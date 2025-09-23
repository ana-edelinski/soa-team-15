import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import Swal from 'sweetalert2';

import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { Tour } from '../../tour-authoring/model/tour.model';
import { TourExecutionService } from '../tour-execution.service';
import { TourExecution } from '../tour-execution.model';


@Component({
  selector: 'xp-purchased-tours',
  templateUrl: './purchased-tours.component.html',
  styleUrls: ['./purchased-tours.component.css']
})
export class PurchasedToursComponent implements OnInit {

  user!: User;
  loading = true;
  tours: Tour[] = [];
  starting: Record<number, boolean> = {}; // po tourId
  isActive: boolean = false;
  activeExecution: TourExecution | null = null;     
  activeTourId?: number;  

  isStarting(t: Tour): boolean {
    const id = t.id as number | undefined;
    return id !== undefined ? !!this.starting[id] : false;
  }

  isContinue(t: Tour): boolean {                    
    const id = t.id as number | undefined;
    return id !== undefined && this.activeTourId === id;
  }

  isDisabled(t: Tour): boolean {                    
    const id = t.id as number | undefined;
    // Disable ako postoji neka aktivna, a ova NIJE aktivna
    return this.activeTourId !== undefined && id !== this.activeTourId;
  }

  buttonLabel(t: Tour): string {                    
    return this.isContinue(t) ? 'Continue' : 'Start tour';
  }


  tourTagMap: { [key: number]: string } = {
    0: 'Cycling',
    1: 'Culture',
    2: 'Adventure',
    3: 'FamilyFriendly',
    4: 'Nature',
    5: 'CityTour',
    6: 'Historical',
    7: 'Relaxation',
    8: 'Wildlife',
    9: 'NightTour',
    10: 'Beach',
    11: 'Mountains',
    12: 'Photography',
    13: 'Guided',
    14: 'SelfGuided'
  };

  constructor(
    private auth: AuthService,
    private execService: TourExecutionService,
    private snack: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.auth.user$.subscribe(async (u) => {
      this.user = u;
      if (!this.user) {
        this.loading = false;
        return;
      }
       try {
      // 1) aktivna egzekucija (pretpostavka: servis vraća null za 404)
      this.activeExecution = await firstValueFrom(this.execService.getActiveExecutionForTourist(this.user.id));
      this.activeTourId = this.activeExecution?.tourId;

      // 2) ture za start (za sada published)
      this.tours = await firstValueFrom(this.execService.getStartableToursForUser(this.user.id));
    } catch (err) {
      console.error(err);
      this.snack.open('Failed to load tours', 'OK', { duration: 3000 });
    } finally {
      this.loading = false;
    }
    });
  }

  async onAction(t: Tour) {                         // ← DODAJ
    if (this.isContinue(t)) {
      // samo vodi u simulator
      this.router.navigate(['/position-simulator']);
      return;
    }
    if (this.isDisabled(t)) {
      this.snack.open('You already have an active tour. Finish or abandon it first.', 'OK', { duration: 3000 });
      return;
    }
    await this.startTour(t); // nema aktivne → normalan start
  }

   async startTour(tour: Tour) {
   const tourId = tour.id as number | undefined;
   //const tourId = 7;
    if(tourId == null){
      return;
    }
    if (!this.user) {
      this.snack.open('You must be logged in as Tourist to start a tour.', 'OK', { duration: 3000 });
      return;
    }
  
    try {
      this.starting[tourId] = true;
      const pos = await firstValueFrom(this.execService.getMyPosition(this.user.id));
  
      if (!pos) {
        const res = await Swal.fire({
          icon: 'info',
          title: 'Set your starting location',
          text: 'Before starting a tour you need to set your current position.',
          showCancelButton: true,
          confirmButtonText: 'Go to Position Simulator',
          cancelButtonText: 'Cancel'
        });
        if (res.isConfirmed) this.router.navigate(['/position-simulator']);
        return; // nema pozicije → ne startujemo
      }

      const payload: TourExecution = {
        tourId,
        touristId: this.user.id,
        locationId: pos.id,
        completedKeys: []
      };
  
      const exec = await firstValueFrom(this.execService.startTourExecution(payload));
      this.activeExecution = exec ?? { ...payload, id:  (exec as any)?.id, status: 'Active', lastActivity: new Date().toISOString() } as any;
      this.activeTourId = this.activeExecution.tourId;

      this.snack.open(`Tour "${tour.name}" started`, 'OK', { duration: 3000 });
      this.router.navigate(['/position-simulator']);
    } catch (err: any) {
      console.error('Failed to start tour', err);
      const msg = err?.error?.message || 'Failed to start tour';
      this.snack.open(msg, 'OK', { duration: 3000 });
    } finally {
      this.starting[tourId] = false;
    }
  }

   tagLabel(t: any): string {
    if (t == null) return '—';
    if (typeof t === 'number') return this.tourTagMap[t] ?? `Tag ${t}`;
    if (typeof t === 'string') return t; // već dobijeno kao tekst
    // fallback (ako backend šalje objekat)
    return String(t?.name ?? t);
  }

  tagImg(t: any): string {
    // tag1.jpg, tag2.jpg, ...
    const idx = ((t?.tags?.[0] ?? 0) + 1);
    return `assets/images/tags/tag${idx}.jpg`;
  }

  onImgErr(ev: Event) {
    (ev.target as HTMLImageElement).src = 'assets/images/tags/default.jpg';
  }


}
