import { Component, OnInit } from '@angular/core';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TourService } from '../../tour-authoring/tour.service';
import { Tour } from '../../tour-authoring/model/tour.model';
import { ReviewDialogComponent } from '../../tour-authoring/review-dialog/review-dialog.component';
import { TourExecutionService } from '../../tour-execution/tour-execution.service';
import { TourExecution } from '../../tour-execution/tour-execution.model';
import { Router } from '@angular/router';
import { CartService } from 'src/app/feature-modules/payments/cart.service';
import { OrderItem } from 'src/app/feature-modules/payments/model/order-item.model';
import { ShoppingCart } from 'src/app/feature-modules/payments/model/shopping-cart.model';

type KeyPoint = { name?: string; description?: string; latitude?: number; longitude?: number };




@Component({
  selector: 'xp-browse-tours',
  templateUrl: './browse-tours.component.html',
  styleUrls: ['./browse-tours.component.css']
})
export class BrowseToursComponent implements OnInit {
  user: User | null = null;
  tours: Tour[] = [];
  loading = false;
  error = '';
  tourExecution: TourExecution = {} as TourExecution;
  tourExecutions: Map<number, TourExecution> = new Map();
  userId: number = -1;
  isActive: boolean = false;
  shoppingCart: ShoppingCart | null = null;
  keyPointsByTour = new Map<number, KeyPoint[]>();

  // ❗ set kupljenih tura – puni se iz PaymentsService-a
  purchasedTourIds = new Set<number>();

  get isTourist(): boolean {
    return !!this.user && this.user.role === 'Tourist';
  }

  constructor(
    private tourService: TourService,
    private tourExecutionService: TourExecutionService,
    private auth: AuthService,
    private router: Router,
    private dialog: MatDialog,
    private snack: MatSnackBar,
    private cartService: CartService
  ) {
    this.auth.user$.subscribe(u => (this.user = u));
  }

  ngOnInit(): void {
    this.loading = true;

    // 1. objavljene ture
    this.tourService.getPublishedTours().subscribe({
      next: data => {
        this.tours = data;
        this.loading = false;

        // === DEBUG: ispisi sta tacno stize iz back-a ===
console.log('%c[Tours] RAW array', 'color:#0aa', data);
console.table(data);

// kljucevi prvog objekta (da vidis koja polja postoje)
console.log('[Tours] keys(first):', Object.keys(data?.[0] || {}));

// praktican table samo sa bitnim poljima
console.table(
  data.map(t => ({
    id: t.id,
    name: t.name,
    lengthInKm: (t as any).lengthInKm,
    durationMinutes: (t as any).durationMinutes,
    startPointName: (t as any).startPointName,
    price: t.price
  }))
);

// stavi na window da mozes u konzoli: __tours, __tours[0], itd.
(window as any).__tours = data;

      },
      error: err => {
        console.error(err);
        this.error = 'Failed to load tours.';
        this.loading = false;
      }
    });

    // 2. user -> korpa + kupljene ture
    this.auth.user$.subscribe(u => {
      this.user = u;
      this.cartService.resetCartState();

      if (u && u.role === 'Tourist') {
        // kupljene ture (PaymentsService)
        this.cartService.getPurchasedTourIds(u.id).subscribe({
        next: ids => {
            this.purchasedTourIds = new Set(ids);
            this.loadKeyPointsForPurchased();   // 🔸 dodato
          },          error: err => console.error('Failed to load purchases', err)
        });

        // korpa
        this.cartService.getCartsByUser(u.id).subscribe({
          next: (carts) => {
            if (carts.length > 0) {
              this.shoppingCart = carts[0];
              this.cartService.getCartItems(this.shoppingCart.id!).subscribe({
                next: () => console.log('Cart items loaded for', u.username),
                error: err => console.error('Failed to load cart items', err)
              });
            } else {
              this.createNewCart(u.id);
            }
          },
          error: (err) => console.error('Failed to load carts for user', err)
        });
      } else {
        this.shoppingCart = null;
        this.purchasedTourIds.clear();
      }
    });
  }

  // ==== “public view” helperi ====

  isPurchased(tour: Tour): boolean {
    return this.purchasedTourIds.has(tour.id!);
  }

  // dužina (km)
  getLengthKm(tour: any): string {
    const v = tour?.lengthInKm ?? tour?.length ?? 0;
    return typeof v === 'number' ? v.toFixed(2) : String(v ?? '—');
    }

  // vreme prolaska – koristi prvo existing polje ako postoji
  getDuration(tour: any): string {
    const minutes =
      tour?.estimatedTimeMinutes ?? tour?.durationMinutes ?? tour?.timeToPassMinutes;
    if (!minutes || isNaN(minutes)) return '—';
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
  }

  // početna tačka – probaj par naziva polja
/*  getStartPointName(tour: any): string {
    return (
      tour?.startPoint?.name ??
      tour?.startPointName ??
      (Array.isArray(tour?.keyPoints) && tour.keyPoints.length > 0
        ? tour.keyPoints[0]?.name
        : null) ??
      '—'
    );
  }*/

/*  // slike
  getImages(tour: any): string[] {
    const imgs = tour?.images ?? tour?.photos ?? [];
    return Array.isArray(imgs) ? imgs : [];
  }*/

  // ključne tačke
 getKeyPoints(tour: any): KeyPoint[] {
  return this.keyPointsByTour.get(tour.id) ?? [];
}

  // ==== UI akcije ====

  openReviewDialog(tour: Tour): void {
    if (!this.user) { this.error = 'You must be logged in.'; return; }
    const ref = this.dialog.open(ReviewDialogComponent, {
      data: { tourId: tour.id!, user: this.user! },
      disableClose: true
    });
    ref.afterClosed().subscribe(created => {
      if (created) this.snack.open('Review submitted ✅', 'OK', { duration: 3000 });
    });
  }

  startTour(tourId: number): void {
    if (!this.user) {
      this.snack.open('You must be logged in as Tourist to start a tour.', 'OK', { duration: 3000 });
      return;
    }
    const execution: TourExecution = {
      tourId: tourId,
      touristId: this.user.id
    };
    this.tourExecutionService.startTourExecution(execution).subscribe({
      next: (data) => {
        console.log('Tour execution started', data);
        this.snack.open(`Tour started`, 'OK', { duration: 3000 });
        this.isActive = true;
        this.router.navigate(['/position-simulator']);
      },
      error: (err) => {
        console.error('Failed to start tour', err);
        this.snack.open('Failed to start tour', 'OK', { duration: 3000 });
      }
    });
  }

  createNewCart(userId: number): void {
    const cart: ShoppingCart = { id: 0, userId, items: [], totalPrice: 0 };
    this.cartService.createShoppingCart(cart).subscribe({
      next: (created) => {
        this.shoppingCart = created;
        console.log('Cart created:', created);
      },
      error: () => this.snack.open('Error creating cart', 'OK', { duration: 3000 })
    });
  }

  addToCart(tour: Tour): void {
    if (!this.user) {
      this.snack.open('You must be logged in as Tourist to add tours to cart.', 'OK', { duration: 3000 });
      return;
    }
    if (!this.shoppingCart) {
      this.snack.open('No active cart. Please refresh or try again.', 'OK', { duration: 3000 });
      return;
    }
    const alreadyInCart = this.cartService['cartItemsSubject'].getValue()
      .some((item: OrderItem) => item.tourId === tour.id);
    if (alreadyInCart) {
      this.snack.open(`${tour.name} is already in your cart ❌`, 'OK', { duration: 3000 });
      return;
    }
    const item: OrderItem = {
      id: 0,
      tourName: tour.name,
      price: tour.price,
      tourId: tour.id!,
      cartId: this.shoppingCart!.id!
    };
    this.cartService.addToCart(item).subscribe({
      next: () => {
        this.snack.open(`${tour.name} added to cart ✅`, 'OK', { duration: 3000 });
        if (this.shoppingCart?.id) this.cartService.getCartItems(this.shoppingCart.id).subscribe();
      },
      error: () => this.snack.open('Failed to add to cart', 'OK', { duration: 3000 })
    });
  }

  // 1) Trajanje: koristi backend polje ako postoji, inače proceni iz dužine (4 km/h)
formatDuration(durationMinutes?: number, lengthKm?: number): string {
  if (durationMinutes && !isNaN(durationMinutes)) {
    const h = Math.floor(durationMinutes / 60);
    const m = durationMinutes % 60;
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
  }
  if (lengthKm && lengthKm > 0) {
    const mins = Math.round((lengthKm / 4) * 60);
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
  }
  return '—';
}

// 2) Početna tačka: koristi polje iz backend-a, pa fallback na 1. keypoint ako je stigao
getStartPointName(tour: any): string {
  return (
    tour?.startPointName ??
    tour?.startPoint?.name ??
    (Array.isArray(tour?.keyPoints) && tour.keyPoints.length > 0
      ? tour.keyPoints[0]?.name
      : null) ??
    '—'
  );
}

// 3) Slike: probaj previewImages, pa images/photos
getImages(tour: any): string[] {
  const imgs = tour?.previewImages ?? tour?.images ?? tour?.photos ?? [];
  return Array.isArray(imgs) ? imgs : [];
}

private loadKeyPointsForPurchased(): void {
  // učitaj tačke SAMO za kupljene ture
  for (const tourId of this.purchasedTourIds) {
    this.tourService.getKeyPointsForTour(String(tourId)).subscribe({
      next: (kps) => this.keyPointsByTour.set(tourId, kps || []),
      error: (err) => console.warn('Failed to load key points for tour', tourId, err)
    });
  }
}


}
