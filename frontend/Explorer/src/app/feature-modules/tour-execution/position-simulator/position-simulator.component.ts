import { Component, OnDestroy, OnInit } from '@angular/core';
import * as L from 'leaflet';
import { MatSnackBar } from '@angular/material/snack-bar';

import { TourExecutionService } from '../tour-execution.service';
import { MapService } from 'src/app/shared/map/map.service';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { TourExecution, TourForTourist } from '../tour-execution.model';
import { interval, of } from 'rxjs';
import { switchMap, filter, tap, catchError } from 'rxjs/operators';

type MapMarker = {
  id?: number;
  lat: number;
  lng: number;
  label?: string;
  description?: string | null;
  image?: string | null;
  completed?: boolean;
};


@Component({
  selector: 'xp-position-simulator',
  templateUrl: './position-simulator.component.html',
  styleUrls: ['./position-simulator.component.css']
})
export class PositionSimulatorComponent implements OnInit, OnDestroy {

  defaultCenter: L.LatLngExpression = [45.2670, 19.8330]; // Novi Sad
  initialMarker?: L.LatLngExpression;

  lastLatLng?: { lat: number; lng: number };
  status: 'idle' | 'saving' | 'saved' | 'error' = 'idle';
  message = '';
  address?: any;
  user!: User;
  currentTour?: TourForTourist;
  routeWaypoints: Array<[number, number]> = [];
  routeMode: 'foot' | 'bike' | 'car' = 'foot';

  private proximitySub?: import('rxjs').Subscription;
  private proximityMeters = 50; // prag blizine (npr. 50m)

  private recomputeRouteWaypoints() {
    this.routeWaypoints = (this.kpMarkers ?? [])
      .map(m => [m.lat, m.lng]) as Array<[number, number]>;
  }

  kpMarkers: MapMarker[] = [];
  private objectUrls: string[] = [];
  private toursApi = 'http://localhost:8082';

  private fileToUrl(file?: File | null): string | null {
    if (file instanceof File) {
      const u = URL.createObjectURL(file);
      this.objectUrls.push(u);
      return u;
    }
    return null;
  }

  private toPublicImageUrl(path: string | null | undefined): string | null {
    if (!path) return null;
    if (/^https?:\/\//i.test(path)) return path;            // već je apsolutan URL
    const cleaned = path.startsWith('/') ? path.slice(1) : path;
    // ako nema '/', to je samo ime fajla -> stavi default folder
    const withFolder = cleaned.includes('/') ? cleaned : `images/keypoints/${cleaned}`;
    return `${this.toursApi}/${withFolder}`;
  }

  private toImageUrl(kp: any): string | null {
    if (typeof kp?.image === 'string' && kp.image) {
      // string sa beka → pretvori u apsolutni URL
      return this.toPublicImageUrl(kp.image);
    }
    // lokalni upload (File) → object URL
    return this.fileToUrl(kp?.pictureFile);                          // tvoj KeyPoint model
  }

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
                next: tour => { 
                  this.currentTour = tour;
                  this.rebuildKeyPointMarkers();     
                  this.recomputeRouteWaypoints();
                  this.startProximityWatcher();
                },
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


  //markeri za kljucne tacke
  

  private isKpCompleted(id?: number): boolean {
    if (id == null) return false;
    return !!this.currentExecution?.completedKeys?.some(k => k.keyPointId === id);
  }

  private rebuildKeyPointMarkers(): void {
    if (!this.currentTour?.keyPoints) { this.kpMarkers = []; return; }

    this.kpMarkers = this.currentTour.keyPoints.map(kp => ({
      id: kp.id,
      lat: kp.latitude,
      lng: kp.longitude,
      label: kp.name,
      description: kp.description ?? '',
      image: this.toImageUrl(kp),           // ako backend šalje url
      completed: this.isKpCompleted(kp.id)
    }));
  }

  ngOnDestroy(): void {
    this.proximitySub?.unsubscribe();
    for (const u of this.objectUrls) URL.revokeObjectURL(u);
    this.objectUrls = [];
  }


  // da li smo blizu kt
  private distanceMeters(aLat: number, aLng: number, bLat: number, bLng: number): number {
    const R = 6371000; // m
    const toRad = (d: number) => d * Math.PI / 180;
    const dLat = toRad(bLat - aLat);
    const dLng = toRad(bLng - aLng);
    const s1 = Math.sin(dLat/2) ** 2 +
              Math.cos(toRad(aLat)) * Math.cos(toRad(bLat)) * Math.sin(dLng/2) ** 2;
    return 2 * R * Math.asin(Math.sqrt(s1));
  }

  private startProximityWatcher() {
    if (!this.user || !this.currentExecution || !this.currentTour) return;
    // očisti stari interval ako postoji
    this.proximitySub?.unsubscribe();

    this.proximitySub = interval(10000).pipe( // svakih 10s
      switchMap(() => this.tourExecutionService.getMyPosition(this.user.id)),
      filter((pos: any) => !!pos),
      tap((pos: any) => {
        // osveži ui (markere/adresu) ako želiš
        this.lastLatLng = { lat: pos.latitude, lng: pos.longitude };
      }),
      // pronađi najbližu nekompletiranu KP u radijusu
      switchMap((pos: any) => {
        const incomplete = (this.currentTour!.keyPoints || [])
          .filter(kp => !this.isKpCompleted(kp.id));

        if (incomplete.length === 0) return of(null);

        // najbliža
        let target = null as any;
        let dist = Infinity;
        for (const kp of incomplete) {
          const d = this.distanceMeters(pos.latitude, pos.longitude, kp.latitude, kp.longitude);
          if (d < dist) { dist = d; target = { kp, d }; }
        }

        if (!target || target.d > this.proximityMeters) return of(null);

        // blizu smo -> pokušaj da kompletiraš
        return this.tourExecutionService.completeKeyPoint(
          this.currentExecution!.id!, target.kp.id!
        ).pipe(
          catchError(err => {
            // 409 (već kompletirana) ili drugi – ignoriši da ne spamuje
            return of(null);
          })
        );
      })
    ).subscribe((updatedExec: any) => {
      if (!updatedExec) return;

      // osveži lokalno stanje exec + markere
      this.currentExecution = updatedExec;
      this.rebuildKeyPointMarkers();
      this.recomputeRouteWaypoints();

      // ako su sve KP kompletirane -> završi turu
      const total = this.currentTour!.keyPoints?.length || 0;
      const done = this.currentExecution?.completedKeys?.length || 0;
      if (total > 0 && done >= total) {
        this.finishTourExecution();
      }
    });
  }

  private finishTourExecution() {
    if (!this.currentExecution?.id) return;
    this.tourExecutionService.completeTourExecution(this.currentExecution.id).subscribe({
      next: exec => {
        this.currentExecution = exec;
        this.snack.open('Tour completed 🎉', 'OK', { duration: 3000 });
        // opcionalno: ugasi watcher i/ili redirect
        this.proximitySub?.unsubscribe();
      },
      error: err => {
        console.error(err);
        this.snack.open('Failed to complete tour', 'OK', { duration: 3000 });
      }
    });
  }
  
  
}
