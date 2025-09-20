import { Component, OnInit } from '@angular/core';
import { TourService } from '../tour.service';
import { Tour } from '../model/tour.model';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { TourStatus } from '../model/tour-status.model';

import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ReviewDialogComponent } from '../review-dialog/review-dialog.component';


@Component({
  selector: 'xp-my-tours',
  templateUrl: './my-tours.component.html',
  styleUrls: ['./my-tours.component.css']
})
export class MyToursComponent implements OnInit {

  tours: Tour[] = [];
  user: User | null = null;
  error_message = '';

  constructor(private tourService: TourService, private authService: AuthService,private dialog: MatDialog,
    private snack: MatSnackBar) {
    this.authService.user$.subscribe(u => this.user = u);
  }

  ngOnInit(): void {
    if (this.user) {
      this.tourService.getToursForAuthor(this.user.id).subscribe({
        next: (data) => this.tours = data,
        error: (err) => {
          console.error(err);
          this.error_message = 'Failed to load tours.';
        }
      });
    }
  }

  getStatusLabel(status: number): string {
    return TourStatus[status];
  }
    openReviewDialog(tour: Tour): void {
    if (!this.user) { this.error_message = 'You must be logged in.'; return; }
    const ref = this.dialog.open(ReviewDialogComponent, {
      data: { tourId: tour.id!, user: this.user! },   // <— ovde ide tvoja linija
      disableClose: true
    });
    ref.afterClosed().subscribe(created => {
      if (created) this.snack.open('Review submitted ✅', 'OK', { duration: 3000 });
    });
  }


    
  
}
