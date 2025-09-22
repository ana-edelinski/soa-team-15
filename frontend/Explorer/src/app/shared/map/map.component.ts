
import { Component, AfterViewInit, OnDestroy, ElementRef, EventEmitter, Input, Output, ViewChild, OnChanges, SimpleChanges } from '@angular/core';
import * as L from 'leaflet';
import 'leaflet-routing-machine';


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
  selector: 'xp-map',
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css']
})
export class MapComponent implements AfterViewInit, OnDestroy, OnChanges {
  @ViewChild('map', { static: true }) mapEl!: ElementRef<HTMLDivElement>;

  @Input() center: L.LatLngExpression = [45.2670, 19.8330]; // NS default
  @Input() zoom = 13;
  @Input() initialMarker?: L.LatLngExpression;
  @Input() markers: MapMarker[] = [];

  @Output() mapClick = new EventEmitter<{ lat: number; lng: number }>();
  @Output() markerClick = new EventEmitter<MapMarker>();

  //inputi za routing
  @Input() routeWaypoints: Array<[number, number]> = [];
  @Input() routeMode: 'foot' | 'bike' | 'car' = 'foot';

  private routingControl?: any; // L.Routing.Control

  private map?: L.Map;
  private marker?: L.Marker;
  private userLayer = L.layerGroup();
  private markersLayer = L.layerGroup();

  constructor(private host: ElementRef) {}

  ngAfterViewInit(): void {
    const container = this.host.nativeElement.querySelector('.map-root');
    this.map = L.map(container).setView(this.center, this.zoom);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap'
    }).addTo(this.map);

    this.userLayer.addTo(this.map);
    this.markersLayer.addTo(this.map);

    if (this.initialMarker) {
      this.marker = L.marker(this.initialMarker).addTo(this.map);
    }
    this.renderKeyPointMarkers();

    this.map.on('click', (e: L.LeafletMouseEvent) => {
      const { lat, lng } = e.latlng;
      if (!this.marker) {
        this.marker = L.marker([lat, lng]).addTo(this.map!);
      } else {
        this.marker.setLatLng([lat, lng]);
      }
      this.mapClick.emit({ lat, lng });
    });

    setTimeout(() => {
      this.map!.invalidateSize();
      this.renderRoute();              // ⬅️ DODAJ OVO
    }, 0);
  }

  /** Omogući da parent postavi/azurira marker programatski */
  public setPosition(lat: number, lng: number) {
    if (!this.map) return;
    if (!this.marker) {
      this.marker = L.marker([lat, lng]).addTo(this.map);
    } else {
      this.marker.setLatLng([lat, lng]);
    }
    this.map.setView([lat, lng], this.map.getZoom());
  }

  ngOnChanges(ch: SimpleChanges) {
    if (!this.map) return;
    if (ch['initialMarker']) {
      const p = this.initialMarker as [number, number] | undefined;
      if (p) this.setPosition(p[0], p[1]);
    }
    if (ch['markers']) this.renderKeyPointMarkers();
    if (ch['routeWaypoints'] || ch['routeMode']) this.renderRoute();
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  private makeKPIcon(completed?: boolean): L.Icon {
    return L.icon({
      iconUrl:  completed
      ? '/assets/images/keypoint-icon-green.png'   // ✅ zelena za kompletirane
      : '/assets/images/keypoint-icon.png', 
      iconSize: [28, 28],
      iconAnchor: [14, 28],
      popupAnchor: [0, -28],
      className: completed ? 'kp-icon kp-icon--completed' : 'kp-icon'
    });
  }

  private buildKPPopup(m: MapMarker): string {
    const img = m.image || 'assets/images/keypoint-icon.png';
    const title = this.escapeHtml(m.label ?? 'Key point');
    const desc = this.escapeHtml(m.description ?? '');
    return `
      <div class="kp-popup">
        <img src="${img}" alt="${title}" />
        <h4>${title}</h4>
        <p>${desc}</p>
      </div>
    `;
  }

  private escapeHtml(s: string): string {
    return s.replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]!));
  }


  private renderKeyPointMarkers() {
    this.markersLayer.clearLayers();
    if (!this.map || !this.markers?.length) return;

    console.log('render KP markers: ', this.markers);

    for (const m of this.markers) {
      const mk = L.marker([m.lat, m.lng], { icon: this.makeKPIcon(m.completed) });
      mk.bindPopup(this.buildKPPopup(m), { maxWidth: 260 });
      mk.on('click', () => this.markerClick.emit(m));
      mk.addTo(this.markersLayer);
    }

    const pts: [number, number][] = this.markers.map(m => [m.lat, m.lng]);
    const u = this.initialMarker as [number, number] | undefined;
    if (u) pts.push(u);
    if (pts.length) this.map.fitBounds(pts as any, { padding: [24, 24] });
  }


  private renderRoute() {
  if (!this.map) return;

  // ukloni staru
  if (this.routingControl) {
    try { this.map.removeControl(this.routingControl); } catch {}
    this.routingControl = undefined;
  }

  if (!this.routeWaypoints || this.routeWaypoints.length < 2) return;

  const wps = this.routeWaypoints.map(([lat, lng]) => L.latLng(lat, lng));

  // OSRM profil (car/bike/foot)
  const profile = this.routeMode; // 'foot'|'bike'|'car'

  this.routingControl = (L as any).Routing.control({
    waypoints: wps,
    router: (L as any).Routing.osrmv1({
      serviceUrl: 'https://router.project-osrm.org/route/v1',
      profile // ako tvoja LRM verzija ignoriše profile, vidi napomenu ispod
    }),
    addWaypoints: false,
    draggableWaypoints: false,
    fitSelectedRoutes: true,
    show: false,
    createMarker: () => null,
    // (opciono) stil linije
    lineOptions: {
      styles: [{ color: '#1310c0ff', weight: 5, opacity: 0.9 }]
    }
  }).addTo(this.map);
}

}
