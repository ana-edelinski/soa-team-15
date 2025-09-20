import { Component, OnInit, AfterViewInit, OnDestroy, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as L from 'leaflet';
import { TourService } from '../tour.service';
import { KeyPoint } from '../model/keypoint.model';
import { Tour } from '../model/tour.model';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-view-key-points',
  templateUrl: './view-key-points.component.html',
  styleUrls: ['./view-key-points.component.css']
})
export class ViewKeyPointsComponent implements OnInit, AfterViewInit, OnDestroy {
  private map?: L.Map;
  private markers: L.Marker[] = [];
  private polyline?: L.Polyline;

  keyPoints: KeyPoint[] = [];
  tour?: Tour;

  // loading / errors
  isLoadingTour = true;
  isLoadingKeyPoints = true;
  errorMsg = '';

  // publish workflow
  isPublishing = false;
  publishErrors: string[] = [];

  // status kodovi (uskladi sa svojim enumom po potrebi)
  private readonly STATUS = { DRAFT: 0, PUBLISHED: 1, ARCHIVED: 2 };

  private readonly TAG_LABELS: string[] = [
    'Cycling',
    'Culture',
    'Adventure',
    'Family Friendly',
    'Nature',
    'City Tour',
    'Historical',
    'Relaxation',
    'Wildlife',
    'Night Tour',
    'Beach',
    'Mountains',
    'Photography',
    'Guided',
    'Self Guided'
  ];

  // Ako backend nekad pošalje string umjesto broja, uljepšaj "PascalCase" -> "Pascal Case"
  private prettifyEnumLabel(s: string): string {
    return s.replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  formatTag(t: number | string): string {
    if (typeof t === 'number') return this.TAG_LABELS[t] ?? `#${t}`;
    return this.prettifyEnumLabel(t);
  }


  private keyPointIcon = L.icon({
    iconUrl: 'assets/images/keypoint-icon.png',
    iconSize: [40, 40],
    iconAnchor: [20, 40],
    popupAnchor: [0, -40],
  });

  constructor(
    private elementRef: ElementRef,
    private route: ActivatedRoute,
    private tourService: TourService
  ) {}

  /** Učitavanje podataka ide u OnInit */
  ngOnInit(): void {
    const tourId = this.route.snapshot.paramMap.get('id');
    if (!tourId) {
      this.errorMsg = 'Tour ID is missing.';
      this.isLoadingTour = false;
      this.isLoadingKeyPoints = false;
      return;
    }

    // TOUR
    this.isLoadingTour = true;
    this.tourService.getTourById(tourId)
      .pipe(finalize(() => (this.isLoadingTour = false)))
      .subscribe({
        next: (t: Tour) => { this.tour = t; },
        error: (err) => {
          console.error('Tour load error', err);
          if (err?.status === 404) this.errorMsg = 'Tour not found.';
          else if (err?.status === 401 || err?.status === 403) this.errorMsg = 'Unauthorized to view this tour.';
          else this.errorMsg = (err?.error?.error ?? err?.error?.detail ?? err?.error ?? 'Failed to load tour.');
        }
      });

    // KEY POINTS
    this.isLoadingKeyPoints = true;
    this.tourService.getKeyPointsForTour(tourId)
      .pipe(finalize(() => (this.isLoadingKeyPoints = false)))
      .subscribe({
        next: (keyPoints: KeyPoint[]) => {
          this.keyPoints = (keyPoints || []).map(kp => ({
            ...kp,
            imageUrl:
              (kp as any).imageUrl ??
              (kp as any).image ??
              (typeof (kp as any).pictureFile === 'string' ? (kp as any).pictureFile : undefined)
          }));
          if (this.map && this.keyPoints.length > 0) {
            this.renderMarkersAndPolyline();
            this.fitMapBounds();
          }
        },
        error: (err) => {
          console.error('Error loading key points', err);
          this.errorMsg = 'Failed to load key points.';
        }
      });
  }

  /** Inicijalizacija mape ide u AfterViewInit */
  ngAfterViewInit(): void {
    this.initializeMap();
    setTimeout(() => this.map?.invalidateSize(), 0);
  }

  ngOnDestroy(): void {
    if (this.map) this.map.remove();
  }

  // ---------- STATUS / PUBLISH AKCIJE ----------

  // ---- STATUS GETTERI ZA TEMPLATE ----
  get isDraft(): boolean {
    return this.getStatusCode(this.tour?.status) === this.STATUS.DRAFT;
  }

  get isPublished(): boolean {
    return this.getStatusCode(this.tour?.status) === this.STATUS.PUBLISHED;
  }

  get isArchived(): boolean {
    return this.getStatusCode(this.tour?.status) === this.STATUS.ARCHIVED;
  }

  get statusLabel(): string {
    const code = this.getStatusCode(this.tour?.status);
    return code === this.STATUS.DRAFT ? 'Draft'
        : code === this.STATUS.PUBLISHED ? 'Published'
        : code === this.STATUS.ARCHIVED ? 'Archived'
        : (this.tour?.status as any)?.toString?.() ?? '';
  }


  /** Normalizuj status iz broja ili stringa */
  private getStatusCode(status: any): number {
    if (typeof status === 'number') return status;
    const map: Record<string, number> = {
      Draft: this.STATUS.DRAFT,
      Published: this.STATUS.PUBLISHED,
      Archived: this.STATUS.ARCHIVED
    };
    return map[String(status)] ?? -1;
  }

  /** Minimalni uslovi na klijentu; backend ostaje izvor istine */
  canPublish(): boolean {
    if (!this.tour) return false;
    const statusOk = this.isDraft; // <— umjesto cast-anja iz statusa
    const hasName = !!this.tour.name?.trim();
    const hasDesc = !!this.tour.description?.trim();
    const hasMinKp = this.keyPoints.length >= 2; // prilagodi ako treba
    return statusOk && hasName && hasDesc && hasMinKp;
  }

  publish(): void {
    if (!this.tour) return;
    this.isPublishing = true;
    this.publishErrors = [];
    const id = String(this.tour.id);

    this.tourService.publishTour(id)
      .pipe(finalize(() => (this.isPublishing = false)))
      .subscribe({
        next: () => {
          // 204 No Content -> samo lokalno promijeni status
          if (typeof (this.tour as any).status === 'number') {
            (this.tour as any).status = this.STATUS.PUBLISHED;
          } else {
            (this.tour as any).status = 'Published';
          }
        },
        error: (err) => {
          const s = err?.error;
          this.publishErrors =
            (s?.errors && Array.isArray(s.errors) && s.errors.length ? s.errors : null)
            ?? (s?.detail ? [s.detail] : null)
            ?? (typeof s === 'string' ? [s] : ['Neuspjelo objavljivanje.']);
        }
      });
  }

  archive(): void {
    if (!this.tour) return;
    this.isPublishing = true;
    this.publishErrors = [];
    const id = String(this.tour.id);

    this.tourService.archiveTour(id)
      .pipe(finalize(() => (this.isPublishing = false)))
      .subscribe({
        next: () => {
          if (typeof (this.tour as any).status === 'number') {
            (this.tour as any).status = this.STATUS.ARCHIVED;
          } else {
            (this.tour as any).status = 'Archived';
          }
        },
        error: (err) => {
          const s = err?.error;
          this.publishErrors =
            (s?.errors && Array.isArray(s.errors) && s.errors.length ? s.errors : null)
            ?? (s?.detail ? [s.detail] : null)
            ?? (typeof s === 'string' ? [s] : ['Neuspjelo arhiviranje.']);
        }
      });
  }

  reactivate(): void {
    if (!this.tour) return;
    this.isPublishing = true;
    this.publishErrors = [];
    const id = String(this.tour.id);

    this.tourService.reactivateTour(id)
      .pipe(finalize(() => (this.isPublishing = false)))
      .subscribe({
        next: () => {
          if (typeof (this.tour as any).status === 'number') {
            (this.tour as any).status = this.STATUS.PUBLISHED;
          } else {
            (this.tour as any).status = 'Published';
          }
        },
        error: (err) => {
          const s = err?.error;
          this.publishErrors =
            (s?.errors && Array.isArray(s.errors) && s.errors.length ? s.errors : null)
            ?? (s?.detail ? [s.detail] : null)
            ?? (typeof s === 'string' ? [s] : ['Neuspjela reaktivacija.']);
        }
      });
  }
  // ---------- MAPA ----------

  private initializeMap(): void {
    const mapContainer: HTMLElement = this.elementRef.nativeElement.querySelector('#view-map');
    if (!mapContainer) return;

    if (!this.map) {
      this.map = L.map(mapContainer).setView([45.2499, 19.8286], 13);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
      }).addTo(this.map);
    }
  }

  /** Očisti stare markere/linije prije novog crtanja */
  private clearMapLayers(): void {
    if (!this.map) return;
    this.markers.forEach(m => this.map!.removeLayer(m));
    this.markers = [];
    if (this.polyline) {
      this.map.removeLayer(this.polyline);
      this.polyline = undefined;
    }
  }

  private renderMarkersAndPolyline(): void {
    if (!this.map || this.keyPoints.length === 0) return;

    this.clearMapLayers();

    this.keyPoints.forEach((kp) => {
      const pos = L.latLng(kp.latitude, kp.longitude);
      const marker = L.marker(pos, { icon: this.keyPointIcon }).addTo(this.map!);

      const popupContent = `
        <div>
          <strong>${kp.name ?? ''}</strong><br/>
          ${kp.description ? `<em>${kp.description}</em><br/>` : ''}
          ${kp.imageUrl ? `<img src="${kp.imageUrl}" width="140" />` : ''}
        </div>
      `;
      marker.bindPopup(popupContent);
      this.markers.push(marker);
    });

    const positions = this.keyPoints.map(kp => L.latLng(kp.latitude, kp.longitude));
    this.polyline = L.polyline(positions, { color: 'blue' }).addTo(this.map);
  }

  private fitMapBounds(): void {
    if (!this.map || this.keyPoints.length === 0) return;
    const bounds = L.latLngBounds(this.keyPoints.map(kp => [kp.latitude, kp.longitude] as [number, number]));
    this.map.fitBounds(bounds, { padding: [50, 50] });
  }

  /** Klik iz liste: zumiraj i otvori popup */
  focusKeyPoint(kp: KeyPoint): void {
    if (!this.map) return;
    const target = this.markers.find(m => {
      const ll = m.getLatLng?.();
      return ll && ll.lat === kp.latitude && ll.lng === kp.longitude;
    });
    const latLng = L.latLng(kp.latitude, kp.longitude);
    this.map.setView(latLng, Math.max(this.map.getZoom(), 15), { animate: true });
    target?.openPopup();
  }

  trackByKpId(index: number, kp: any) {
    return kp.id ?? `${kp.latitude},${kp.longitude},${index}`;
  }
}
