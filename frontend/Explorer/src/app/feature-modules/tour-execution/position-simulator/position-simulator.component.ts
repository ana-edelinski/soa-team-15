import { Component, OnInit } from '@angular/core';
import * as L from 'leaflet';
import { MatSnackBar } from '@angular/material/snack-bar';

import { TourExecutionService } from '../tour-execution.service';
import { MapService } from 'src/app/shared/map/map.service';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { TourExecution } from '../tour-execution.model';

@Component({
  selector: 'xp-position-simulator',
  templateUrl: './position-simulator.component.html',
  styleUrls: ['./position-simulator.component.css']
})
export class PositionSimulatorComponent implements OnInit {

  defaultCenter: L.LatLngExpression = [45.2670, 19.8330]; // Novi Sad
  initialMarker?: L.LatLngExpression;

  lastLatLng?: { lat: number; lng: number };
  status: 'idle' | 'saving' | 'saved' | 'error' = 'idle';
  message = '';
  address?: any;
  user!: User;

  private saving = false;

  // stanje ture
  isActive = false; //sad je na false treba proveriti da li korisnik ima aktivnu turu da bi se ucitala
  currentExecution: TourExecution | null = null;

  constructor(
    private tourExecutionService: TourExecutionService,
    private mapService: MapService,
    private authService: AuthService,
    private snack: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      this.user = user;
    });
  }

  onMapClick(ev: { lat: number; lng: number }) {
    this.lastLatLng = ev;

    // 1) backend update (auto-save)
    if (!this.saving) {
      this.saving = true;
      this.tourExecutionService.updateMyPosition(ev.lat, ev.lng, this.user.id).subscribe({
        next: () => (this.saving = false),
        error: () => (this.saving = false) // tiho
      });
    }

    // 2) reverse geocoding
    this.mapService.reverse(ev.lat, ev.lng).subscribe({
      next: data => (this.address = data?.address),
      error: () => (this.address = undefined)
    });
  }

  abandonTour(id: number): void {
    this.tourExecutionService.abandonTourExecution(id).subscribe({
      next: (data) => {
        console.log('Tour abandoned ✅', data);
        this.snack.open('Tour abandoned ❌', 'OK', { duration: 3000 });
        this.isActive = false;
        this.currentExecution = null;
      },
      error: (err) => {
        console.error('Failed to abandon tour', err);
        this.snack.open('Failed to abandon tour', 'OK', { duration: 3000 });
      }
    });
  }
}
