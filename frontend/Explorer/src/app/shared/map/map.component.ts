
import { Component, AfterViewInit, OnDestroy, ElementRef, EventEmitter, Input, Output } from '@angular/core';
import * as L from 'leaflet';

@Component({
  selector: 'xp-map',
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css']
})
export class MapComponent implements AfterViewInit, OnDestroy {


  @Input() center: L.LatLngExpression = [45.2670, 19.8330]; // NS default
  @Input() zoom = 13;
  @Input() initialMarker?: L.LatLngExpression;

  @Output() mapClick = new EventEmitter<{ lat: number; lng: number }>();

  private map?: L.Map;
  private marker?: L.Marker;

  constructor(private host: ElementRef) {}

  ngAfterViewInit(): void {
    const container = this.host.nativeElement.querySelector('.map-root');
    this.map = L.map(container).setView(this.center, this.zoom);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap'
    }).addTo(this.map);

    if (this.initialMarker) {
      this.marker = L.marker(this.initialMarker).addTo(this.map);
    }

    this.map.on('click', (e: L.LeafletMouseEvent) => {
      const { lat, lng } = e.latlng;
      if (!this.marker) {
        this.marker = L.marker([lat, lng]).addTo(this.map!);
      } else {
        this.marker.setLatLng([lat, lng]);
      }
      this.mapClick.emit({ lat, lng });
    });
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

  ngOnDestroy(): void {
    this.map?.remove();
  }
}
