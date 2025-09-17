  import { Component, OnInit } from '@angular/core';
  import * as L from 'leaflet';
  import { MatSnackBar } from '@angular/material/snack-bar';

  import { TourExecutionService } from '../tour-execution.service';
  import { MapService } from 'src/app/shared/map/map.service';
  import { AuthService } from 'src/app/infrastructure/auth/auth.service';
  import { User } from 'src/app/infrastructure/auth/model/user.model';
  import { TourExecution, TourForTourist } from '../tour-execution.model';
import { Tour } from '../../tour-authoring/model/tour.model';

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
    currentTour?: TourForTourist;

    private saving = false;
    showMap = false;

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

        if (this.user) {
          this.tourExecutionService.getActiveExecutionForTourist(this.user.id).subscribe({
            next: (execution) => {
              if (execution) {
                this.currentExecution = execution;
                this.isActive = true;
                console.log('Active tour loaded:', execution);

                //dobavi pozociju korisnika
                this.tourExecutionService.getMyPosition(this.user.id).subscribe({
                  next: pos => {
                    if (pos) {
                      this.setPosition(pos.latitude, pos.longitude, /*withReverse*/ true);
                    } else {
                      // ipak prikaži mapu i bez markera
                      this.showMap = true;
                    }
                  },
                  error: _ => { this.showMap = true; }
                });
                 this.tourExecutionService.getTourInfoForExecution(execution.tourId).subscribe({
                  next: tour => this.currentTour = tour,
                  error: err => console.error('Failed to load tour', err)
                });

              }
            },
            error: (err) => {
              if (err.status === 404) {
                console.log('No active tour for this user');
              } else {
                console.error('Failed to load active execution', err);
              }
            }
          });
        }
      });
    }

    private setPosition(lat: number, lng: number, withReverse = false) {
      this.initialMarker = [lat, lng];
      this.defaultCenter = [lat, lng];
      this.lastLatLng = { lat, lng };

      if (withReverse) {
        this.mapService.reverse(lat, lng).subscribe({
          next: data => (this.address = data?.address),
          error: () => (this.address = undefined)
        });
      }

      // Ako <xp-map> čita initialMarker samo pri mountu, pobrini se da je mount sada
      if (!this.showMap) {
        // mali delay da Angular pokupi inpute pre mounta
        setTimeout(() => (this.showMap = true));
      }
    }

    onMapClick(ev: { lat: number; lng: number }) {
      this.setPosition(ev.lat, ev.lng, /*withReverse*/ true);

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

     private tourTagMap: Record<number, string> = {
        0: 'Cycling', 1: 'Culture', 2: 'Adventure', 3: 'FamilyFriendly',
        4: 'Nature', 5: 'CityTour', 6: 'Historical', 7: 'Relaxation',
        8: 'Wildlife', 9: 'NightTour', 10: 'Beach', 11: 'Mountains',
        12: 'Photography', 13: 'Guided', 14: 'SelfGuided'
      };

    tagLabel(t: any): string {
      if (t == null) return '—';
      if (typeof t === 'number') return this.tourTagMap[t] ?? `Tag ${t}`;
      return t;
    }
  }
