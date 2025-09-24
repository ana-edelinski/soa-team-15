import { Component, OnInit, AfterViewInit, OnDestroy, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as L from 'leaflet';
import { TourService } from '../tour.service';
import { KeyPoint } from '../model/keypoint.model';
import { Tour } from '../model/tour.model';
import { TransportTimeDto, TransportType } from '../tour.service';

import { finalize } from 'rxjs/operators';
import { TokenStorage } from 'src/app/infrastructure/auth/jwt/token.service';

@Component({
  selector: 'app-view-key-points',
  templateUrl: './view-key-points.component.html',
  styleUrls: ['./view-key-points.component.css']
})



export class ViewKeyPointsComponent implements OnInit, AfterViewInit, OnDestroy {
  private map?: L.Map;
  private markers: L.Marker[] = [];
  private polyline?: L.Polyline;


  currentUserId: number | null = null;

  transportTimes: TransportTimeDto[] = [];    
  transportTime?: TransportTimeDto; 


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
    private tourService: TourService,
    private tokenStorage: TokenStorage,
    
  ) {}

  /** Učitavanje podataka ide u OnInit */
  ngOnInit(): void {
        console.log('Current user ID:', this.currentUserId);

     this.extractCurrentUserIdFromJwt();
    console.log('Current user ID:', this.currentUserId);




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


    
      this.tourService.getTransportTimes(tourId)
        .subscribe({
          next: (list) => {
            this.transportTimes = list ?? [];
            this.transportTime = this.transportTimes[0]; // ako u UI prikazuješ jedan zapis
          },
          error: () => {
            this.transportTimes = [];
            this.transportTime = undefined;
          }
        });
      
  }

    private extractCurrentUserIdFromJwt(): void {
    const token = this.tokenStorage.getAccessToken?.();
    if (!token) {
      this.currentUserId = null;
      return;
    }
    try {
      const payloadBase64 = token.split('.')[1]
        .replace(/-/g, '+')
        .replace(/_/g, '/');
      const json = JSON.parse(atob(payloadBase64));
      // pokrivamo razne tipične ključeve
      const candidates = ['id', 'userId', 'nameid', 'sub'];
      for (const k of candidates) {
        if (json?.[k] !== undefined && json?.[k] !== null) {
          this.currentUserId = json[k];
          break;
        }
      }
    } catch {
      this.currentUserId = null;
    }
  }



  private refreshTourById(id: string) {
    this.tourService.getTourById(id).subscribe({
      next: (fresh: Tour) => {
        // ako servis već mapira Pascal→camel, ovo je dovoljno:
        this.tour = { ...this.tour, ...fresh };

        // fallback ako backend šalje PascalCase
        (this.tour as any).publishedTime = (fresh as any).publishedTime ?? (fresh as any).PublishedTime ?? null;
        (this.tour as any).archiveTime   = (fresh as any).archiveTime   ?? (fresh as any).ArchiveTime   ?? null;
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
  if (!this.tour?.id) return;
  this.isPublishing = true;
  this.publishErrors = [];
  const id = String(this.tour.id);

  this.tourService.publishTour(id)
    .pipe(finalize(() => (this.isPublishing = false)))
    .subscribe({
      next: (res: any) => {
        // status lokalno
        (this.tour as any).status = this.STATUS.PUBLISHED;
        // ako API vraća { publishedTime }, upiši; u suprotnom refetch
        if (res?.publishedTime) {
          (this.tour as any).publishedTime = res.publishedTime;
          (this.tour as any).archiveTime = null;
        } else {
          this.refreshTourById(id);
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
  if (!this.tour?.id) return;
  this.isPublishing = true;
  this.publishErrors = [];
  const id = String(this.tour.id);

  this.tourService.archiveTour(id)
    .pipe(finalize(() => (this.isPublishing = false)))
    .subscribe({
      next: (res: any) => {
        (this.tour as any).status = this.STATUS.ARCHIVED;
        if (res?.archiveTime) {
          (this.tour as any).archiveTime = res.archiveTime;
        } else {
          this.refreshTourById(id);
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
  if (!this.tour?.id) return;
  this.isPublishing = true;
  this.publishErrors = [];
  const id = String(this.tour.id);

  this.tourService.reactivateTour(id)
    .pipe(finalize(() => (this.isPublishing = false)))
    .subscribe({
      next: () => {
        (this.tour as any).status = this.STATUS.PUBLISHED;
        // reaktivacija briše archiveTime; za svaki slučaj i refetch
        (this.tour as any).archiveTime = null;
        this.refreshTourById(id);
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

  

  formatTransportTypeLabel(t?: TransportType | number | string): string {
  if (t === undefined || t === null) return '—';
  const map: Record<string, string> = {
    '0': 'walk',
    '1': 'bike',
    '2': 'car',
    Walk: 'walk',
    Bike: 'bike',
    Car: 'car'
  };
  return map[String(t)] ?? '—';
}





private base64UrlDecode(input: string): string {
  // decode JWT payload (url-safe base64)
  input = input.replace(/-/g, '+').replace(/_/g, '/');
  const pad = input.length % 4;
  if (pad) input += '='.repeat(4 - pad);
  return decodeURIComponent(
    atob(input).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')
  );
}

private readUserIdFromLocalStorageDirect(): number | null {

  // ako si negdje direktno snimila userId poslije login-a
  const raw = localStorage.getItem('userId') ?? localStorage.getItem('currentUserId');
  console.log('LOCAL STORAGE user ID:', this.currentUserId);

  return raw != null ? Number(raw) : null;
}

get ownerId(): number | null {
  // tvoj Tour već ima userId (vidim u logu)
  const raw = (this.tour as any)?.userId ?? (this.tour as any)?.UserId ?? null;
  return raw != null ? Number(raw) : null;
}

get isOwner(): boolean {
  return this.ownerId != null && this.currentUserId != null &&
         Number(this.ownerId) === Number(this.currentUserId);
}


}



