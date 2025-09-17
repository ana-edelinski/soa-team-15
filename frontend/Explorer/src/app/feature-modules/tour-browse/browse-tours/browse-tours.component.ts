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
import Swal from 'sweetalert2';
import { Router } from '@angular/router';
import { CartService } from 'src/app/feature-modules/payments/cart.service';
import { OrderItem } from 'src/app/feature-modules/payments/model/order-item.model';
import { ShoppingCart } from 'src/app/feature-modules/payments/model/shopping-cart.model';


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

  // 1. Učitaj ture
  this.tourService.getPublishedTours().subscribe({
    next: data => { 
      this.tours = data; 
      this.loading = false; 
    },
    error: err => { 
      console.error(err); 
      this.error = 'Failed to load tours.'; 
      this.loading = false; 
    }
  });

  // 2. Reaguj na promenu usera
  this.auth.user$.subscribe(u => {
    this.user = u;
    this.cartService.resetCartState();   // 🧹 očisti staro stanje

    if (u && u.role === 'Tourist') {
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
        error: (err) => {
          console.error('Failed to load carts for user', err);
        }
      });
    } else {
      this.shoppingCart = null;
    }
  });
}


  // (opciono) lokalna provera – pravo ostavljanja recenzije
  // Specifikacija kaže da recenzija sadrži "datum kada je posetio turu",
  // pa je najčistije dozvoliti recenziju ako postoji kupovina ili završena sesija.
  // Ako backend to već striktno proverava, možeš samo vratiti true i prepustiti serveru.
  canReview(tour: Tour): boolean {
    if (!this.isTourist) return false;
    // Minimalna klijentska logika; backend neka bude izvor istine:
    return true;
  }

  openReviewDialog(tour: Tour): void {
    if (!this.user) { this.error = 'You must be logged in.'; return; }
   // if (!this.canReview(tour)) { this.snack.open('Only tourists can review tours.', 'OK', { duration: 3000 }); return; }

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
    // locationId i ostalo dodati kasnije kada se poveze sa simulatorom
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
    const cart: ShoppingCart = {
      id: 0, // backend ga kreira
      userId: userId,
      items: [],
      totalPrice: 0
    };

    this.cartService.createShoppingCart(cart).subscribe({
      next: (created) => {
        this.shoppingCart = created;
        console.log('Cart created:', created);
      },
      error: () => {
        this.snack.open('Error creating cart', 'OK', { duration: 3000 });
      }
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

    if (!this.user || !this.shoppingCart) return;

    const alreadyInCart = this.cartService['cartItemsSubject'].getValue()
    .some(item => item.tourId === tour.id);

    if (alreadyInCart) {
      this.snack.open(`${tour.name} is already in your cart ❌`, 'OK', { duration: 3000 });
      return;
    }


    const item: OrderItem = {
      id: 0, // backend kreira
      tourName: tour.name,
      price: tour.price,
      tourId: tour.id!,
      cartId: this.shoppingCart!.id!   // ✅ sada TS zna da neće biti undefined
    };


    this.cartService.addToCart(item).subscribe({
      next: (created) => {
        this.snack.open(`${tour.name} added to cart ✅`, 'OK', { duration: 3000 });
        console.log('Added to cart', created);

        // ✅ prvo proveri da li postoji ID
        if (this.shoppingCart?.id) {
          this.cartService.getCartItems(this.shoppingCart.id).subscribe();
        } else {
          console.error('Cart has no valid ID!');
        }
      },
      error: (err) => {
        console.error('Failed to add to cart', err);
        this.snack.open('Failed to add to cart', 'OK', { duration: 3000 });
      }
    });

  }

}
