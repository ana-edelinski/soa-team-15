import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TourService } from '../tour.service';
import { Tour } from '../model/tour.model';

@Component({
  selector: 'app-tours-list',
  templateUrl: './tours-list.component.html',
  styleUrls: ['./tours-list.component.css']
})
export class ToursListComponent implements OnInit {
  tours: Tour[] = [];
  loading = false;
  error = '';

  constructor(private tourService: TourService, private router: Router) {}

  ngOnInit(): void {
    this.loadAllToursAllStatuses();
  }

  private loadAllToursAllStatuses(): void {
    this.loading = true;
    this.error = '';

    this.tourService.getAllIncludingUnpublished().subscribe({
      next: (data: Tour[]) => {
        this.tours = Array.isArray(data) ? data : [];
        this.loading = false;
      },
      error: (err) => {
        if (err?.status === 403) this.error = 'Nemaš dozvolu (Admin only).';
        else if (err?.status === 401) this.error = 'Nedostaje/istekao token (401).';
        else this.error = err?.error?.error || err?.message || 'Greška pri učitavanju tura.';
        this.loading = false;
      }
    });
  }

  goToKeyPoints(tourId: number): void {
    this.router.navigate(['/tours', tourId, 'key-points', 'view']);
  }

  trackById = (_: number, t: Tour) => t?.id ?? _;
}
