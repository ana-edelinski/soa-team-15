import { Component, OnInit, AfterViewInit, OnDestroy, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as L from 'leaflet';
import { TourService } from '../tour.service';
import { PublicTour, PublicKeyPoint } from '../model/tour-public.model';

@Component({
  selector: 'app-public-tour-detail',
  templateUrl: './public-tour-detail.component.html',
  styleUrls: ['./public-tour-detail.component.css']
})
export class PublicTourDetailComponent implements OnInit, AfterViewInit, OnDestroy {
  tour?: PublicTour;
  kp?: PublicKeyPoint;
  loading = true;
  error = '';

  private map?: L.Map;
  private marker?: L.Marker;

  private readonly TAG_LABELS: string[] = [
    'Cycling','Culture','Adventure','Family Friendly','Nature','City Tour','Historical',
    'Relaxation','Wildlife','Night Tour','Beach','Mountains','Photography','Guided','Self Guided'
  ];

  constructor(
    private route: ActivatedRoute,
    private el: ElementRef,
    private tourService: TourService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.error = 'Nedostaje ID ture.'; this.loading = false; return; }

    this.tourService.getPublicTourById(id).subscribe({
      next: (t) => {
        this.tour = t;
        this.kp = t?.firstKeyPoint || undefined;
        this.loading = false;
        this.renderMarker();
      },
      error: (e) => {
        this.error = e?.error?.error || e?.message || 'Greška pri učitavanju ture.';
        this.loading = false;
      }
    });
  }

  ngAfterViewInit(): void {
    this.initMap();
    setTimeout(() => this.map?.invalidateSize(), 0);
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  // -------- UI helpers --------
  get keyPointCount(): number { return this.kp ? 1 : 0; }

  formatTag(t: number | string): string {
    if (typeof t === 'number') return this.TAG_LABELS[t] ?? `#${t}`;
    return String(t).replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  // -------- MAPA --------
  private initMap(): void {
    const host: HTMLElement = this.el.nativeElement.querySelector('#pub-map');
    if (!host) return;
    if (!this.map) {
      this.map = L.map(host).setView([45.2499, 19.8335], 13);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
      }).addTo(this.map);
    }
    this.renderMarker();
  }

  private renderMarker(): void {
    if (!this.map || !this.kp) return;
    if (this.marker) { this.map.removeLayer(this.marker); this.marker = undefined; }
    const ll = L.latLng(this.kp.latitude, this.kp.longitude);
    this.marker = L.marker(ll).addTo(this.map)
      .bindPopup(`<strong>${this.kp.name ?? ''}</strong><br/>Lat ${this.kp.latitude.toFixed(5)}, Lng ${this.kp.longitude.toFixed(5)}`);
    this.map.setView(ll, 15);
  }


  focusFirst(): void {
  if (!this.map || !this.kp) return;
  const ll = L.latLng(this.kp.latitude, this.kp.longitude);
  this.map.setView(ll, Math.max(this.map.getZoom(), 15), { animate: true });
  this.marker?.openPopup();
}
}
