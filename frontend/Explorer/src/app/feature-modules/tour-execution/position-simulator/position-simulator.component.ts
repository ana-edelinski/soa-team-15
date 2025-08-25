import { Component } from '@angular/core';
import { TourExecutionService } from '../tour-execution.service';
import * as L from 'leaflet';
import { MapService } from 'src/app/shared/map/map.service';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';

@Component({
  selector: 'xp-position-simulator',
  templateUrl: './position-simulator.component.html',
  styleUrls: ['./position-simulator.component.css']
})
export class PositionSimulatorComponent {

  defaultCenter: L.LatLngExpression = [45.2670, 19.8330]; // Novi Sad
  initialMarker?: L.LatLngExpression;

  lastLatLng?: { lat: number; lng: number };
  status: 'idle' | 'saving' | 'saved' | 'error' = 'idle';
  message = '';
  address?: any;
  user: User;

  private saving = false;

  constructor(private service: TourExecutionService, private mapService: MapService, private authService: AuthService) {}

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
      console.log("id: " + this.user.id)
      this.service.updateMyPosition(ev.lat, ev.lng, this.user.id).subscribe({
        next: () => (this.saving = false),
        error: () => (this.saving = false) // tiho
      });
    }

    // 2) reverse geocoding preko servisa
    this.mapService.reverse(ev.lat, ev.lng).subscribe({
      next: data => (this.address = data?.address),
      error: () => (this.address = undefined)
    });
  }

}
