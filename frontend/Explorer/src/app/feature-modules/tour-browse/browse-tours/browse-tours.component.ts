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

  get isTourist(): boolean {
    return !!this.user && this.user.role === 'Tourist';
  }


  constructor(
    private tourService: TourService,
    private tourExecutionService: TourExecutionService,
    private auth: AuthService,
    private router: Router,
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

}
