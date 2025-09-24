import { Component, OnInit } from '@angular/core';
import { TourService } from '../tour.service';
import { PublicTour } from '../model/tour-public.model';
import { TokenStorage } from 'src/app/infrastructure/auth/jwt/token.service';

@Component({
  selector: 'app-public-tours-list',
  templateUrl: './public-tours-list.component.html',
  styleUrls: ['./public-tours-list.component.css']
})
export class PublicToursListComponent implements OnInit {
  tours: PublicTour[] = [];
  loading = false;
  error = '';

  // auth info
  currentUserId: number | null = null;
  currentUserRoles: string[] = [];

  constructor(
    private tourService: TourService,
    private tokenStorage: TokenStorage
  ) {}

  ngOnInit(): void {
    this.loading = true;

    this.extractAuthFromJwt();

    if (!this.isTourist) {
      this.loading = false;
      this.error = 'Ova lista je dostupna samo korisnicima sa ulogom "Tourist".';
      return;
    }

    this.tourService.getPublicTours().subscribe({
      next: (data) => { this.tours = data ?? []; this.loading = false; },
      error: (e) => { this.error = e?.error?.error || e?.message || 'Greška pri učitavanju.'; this.loading = false; }
    });
  }

  trackById = (_: number, t: PublicTour) => t?.id ?? _;

  // ---------- AUTH HELPERS ----------

  get isTourist(): boolean {
    return this.currentUserRoles.some(r => r.toLowerCase() === 'tourist');
  }

  private extractAuthFromJwt(): void {
    const token = this.tokenStorage.getAccessToken?.();
    if (!token) { this.currentUserId = null; this.currentUserRoles = []; return; }

    try {
      const payloadBase64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = JSON.parse(atob(payloadBase64));

      // ID kandidati
      for (const k of ['id', 'userId', 'nameid', 'sub']) {
        if (json?.[k] !== undefined && json?.[k] !== null) {
          this.currentUserId = Number(json[k]);
          break;
        }
      }

      // ROLES kandidati
      const roleKeys = ['role', 'roles', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      const found: string[] = [];
      for (const rk of roleKeys) {
        const v = json?.[rk];
        if (typeof v === 'string') found.push(v);
        else if (Array.isArray(v)) found.push(...v.filter(x => typeof x === 'string'));
      }
      this.currentUserRoles = [...new Set(found.map(r => String(r)))];
    } catch {
      this.currentUserId = null;
      this.currentUserRoles = [];
    }
  }
}
