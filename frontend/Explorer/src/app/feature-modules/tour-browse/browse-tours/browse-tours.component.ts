import { Component, OnInit } from '@angular/core';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TourService } from '../../tour-authoring/tour.service';
import { Tour } from '../../tour-authoring/model/tour.model';
import { ReviewDialogComponent } from '../../tour-authoring/review-dialog/review-dialog.component';

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

  get isTourist(): boolean {
    return !!this.user && (this.user.role === 'TOURIST' || this.user.role === 'Turista' || this.user.role ===  'ROLE_TOURIST');
  }

  constructor(
    private tourService: TourService,
    private auth: AuthService,
    private dialog: MatDialog,
    private snack: MatSnackBar
  ) {
    this.auth.user$.subscribe(u => (this.user = u));
  }

  ngOnInit(): void {
    this.loading = true;
    this.tourService.getPublishedTours().subscribe({
      next: data => { this.tours = data; this.loading = false; },
      error: err => { console.error(err); this.error = 'Failed to load tours.'; this.loading = false; }
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
}
