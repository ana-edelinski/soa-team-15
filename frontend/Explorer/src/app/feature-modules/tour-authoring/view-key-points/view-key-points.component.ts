import { Component, AfterViewInit, OnDestroy, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as L from 'leaflet';
import { TourService } from '../tour.service';
import { KeyPoint } from '../model/keypoint.model';

@Component({
  selector: 'app-view-key-points',
  templateUrl: './view-key-points.component.html',
  styleUrls: ['./view-key-points.component.css']
})
export class ViewKeyPointsComponent implements AfterViewInit, OnDestroy {
  private map?: L.Map;
  private markers: L.Marker[] = [];
  private polyline?: L.Polyline;
  keyPoints: KeyPoint[] = [];

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

  ngAfterViewInit(): void {
    this.initializeMap();

    const tourId = this.route.snapshot.paramMap.get('id');
    if (tourId) {
      this.loadKeyPoints(tourId);  // Load key points once the map is ready
    }
  }

  ngOnDestroy(): void {
    if (this.map) {
      this.map.remove();
    }
  }

  private initializeMap(): void {
    const mapContainer = this.elementRef.nativeElement.querySelector('#view-map');
    if (!this.map) {
      this.map = L.map(mapContainer).setView([45.2499, 19.8286], 13); // Default coordinates
    }

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution:
        '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(this.map);
  }

  private loadKeyPoints(tourId: string): void {
    this.tourService.getKeyPointsForTour(tourId).subscribe(
      (keyPoints: KeyPoint[]) => {
        this.keyPoints = keyPoints;  // Assign the result to the keyPoints array
        if (this.keyPoints.length > 0) {
          this.renderMarkersAndPolyline();
          this.fitMapBounds();
        }
      },
      (err) => {
        console.error('Error loading key points', err);
      }
    );
  }

  private renderMarkersAndPolyline(): void {
    if (!this.map || this.keyPoints.length === 0) return;  // Ensure map is initialized and keyPoints are available

    this.keyPoints.forEach((kp) => {
      const pos = L.latLng(kp.latitude, kp.longitude);

      const marker = L.marker(pos, { icon: this.keyPointIcon }).addTo(this.map!);

      const popupContent = `
        <div>
          <strong>${kp.name}</strong><br/>
          <em>${kp.description}</em><br/>
          ${kp.pictureFile ? `<img src="${kp.pictureFile}" width="120" />` : ''}
        </div>
      `;
      marker.bindPopup(popupContent);

      this.markers.push(marker);
    });

    const positions = this.keyPoints.map((kp) => L.latLng(kp.latitude, kp.longitude));
    this.polyline = L.polyline(positions, { color: 'blue' }).addTo(this.map);
  }

  private fitMapBounds(): void {
    if (!this.map || this.keyPoints.length === 0) return;

    const bounds = L.latLngBounds(this.keyPoints.map((kp) => [kp.latitude, kp.longitude]));
    this.map.fitBounds(bounds, { padding: [50, 50] });
  }
}
