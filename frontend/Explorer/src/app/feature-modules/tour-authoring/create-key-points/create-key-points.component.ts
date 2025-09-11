import { Component, AfterViewInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';

import * as L from 'leaflet';
import { TourService } from '../tour.service';
import { KeyPoint } from '../model/keypoint.model';



@Component({
  selector: 'app-create-key-points',
  templateUrl: './create-key-points.component.html',
  styleUrls: ['./create-key-points.component.css']
})
export class CreateKeyPointsComponent implements AfterViewInit, OnDestroy {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  private map?: L.Map;
  private markers: L.Marker[] = [];
  private polyline?: L.Polyline;
  
  keyPoints: KeyPoint[] = [];
  selectedKeyPointIndex: number | null = null;
  errorMessage: string = '';
  mapCenter: L.LatLng = L.latLng(45.249891507593155, 19.828605651855472);
  
  private tourId: string | null;
  private token: string | null;

  private keyPointIcon = L.icon({
    iconUrl: 'assets/images/keypoint-icon.png', 
    iconSize: [40, 40],
    iconAnchor: [20, 40],
    popupAnchor: [0, -40],
    shadowUrl: '',
    shadowSize: [0, 0],
    shadowAnchor: [0, 0]
  });

  constructor(
    private elementRef: ElementRef,
    private http: HttpClient,
    private tourService: TourService,
    private router: Router,
    private route: ActivatedRoute 

  ) {
    this.token = localStorage.getItem('token');
    this.tourId = localStorage.getItem('tourId');
  }
  ngOnInit(): void {
    this.tourId = this.route.snapshot.paramMap.get('id'); // 'id' matches your route path
  }

  ngAfterViewInit(): void {
    this.initializeMap();
    this.addInitialMarker();
  }

  ngOnDestroy(): void {
    if (this.map) {
      this.map.remove();
    }
  }

  private initializeMap(): void {
    const mapContainer = this.elementRef.nativeElement.querySelector('#map');
    this.map = L.map(mapContainer).setView(this.mapCenter, 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(this.map);

    this.map.on('moveend', () => {
      if (this.map) {
        this.mapCenter = this.map.getCenter();
      }
    });
  }

  private addInitialMarker(): void {
    const initialKeyPoint: KeyPoint = {
      latitude: this.mapCenter.lat,
      longitude: this.mapCenter.lng,
      name: '',
      description: '',
      pictureFile: null,      
    };

    this.keyPoints.push(initialKeyPoint);
    this.createMarker(this.mapCenter, this.keyPoints.length - 1);
    this.updatePolyline();
    this.selectedKeyPointIndex = 0;
  }

  addMarker(): void {
    if (!this.map) return;

    const center = this.map.getCenter();
    
    const keyPoint: KeyPoint = {
      latitude: center.lat,
      longitude: center.lng,
      name: '',
      description: '',
      pictureFile: null
    };

    this.keyPoints.push(keyPoint);
    const newIndex = this.keyPoints.length - 1;
    
    this.createMarker(center, newIndex);
    this.updatePolyline();
    this.selectedKeyPointIndex = newIndex;
  }

  private createMarker(position: L.LatLng, index: number): void {
    if (!this.map) return;

    const marker = L.marker(position, { 
      draggable: true,
      icon: this.keyPointIcon
    }).addTo(this.map);

    // Set initial popup
    this.updateMarkerPopup(marker, index);

    // Handle drag events
    marker.on('dragend', () => {
      const newPos = marker.getLatLng();
      this.updateMarkerPosition(index, newPos);
    });

    // Handle click events
    marker.on('click', () => {
      this.handleMarkerClick(index);
    });

    this.markers[index] = marker;
  }

  private updateMarkerPopup(marker: L.Marker, index: number): void {
    const keyPointName = this.keyPoints[index]?.name || `Key Point ${index + 1}`;
    
    const popupContent = document.createElement('div');
    popupContent.innerHTML = `
      <div>
        <strong>${keyPointName}</strong><br/>
        <button class="btn btn-danger btn-sm remove-btn">Remove point</button>
      </div>
    `;

    // Add click event to remove button
    const removeBtn = popupContent.querySelector('.remove-btn');
    if (removeBtn) {
      removeBtn.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        this.removeMarker(index);
      });
    }

    marker.bindPopup(popupContent);
  }

  private updateMarkerPosition(index: number, newPosition: L.LatLng): void {
    if (this.keyPoints[index]) {
      this.keyPoints[index].latitude = newPosition.lat;
      this.keyPoints[index].longitude = newPosition.lng;
      this.updatePolyline();
    }
  }

  private updatePolyline(): void {
    if (!this.map) return;

    const positions = this.keyPoints.map(kp => L.latLng(kp.latitude, kp.longitude));
    
    if (this.polyline) {
      this.polyline.setLatLngs(positions);
    } else if (positions.length > 0) {
      this.polyline = L.polyline(positions, { color: 'blue' }).addTo(this.map);
    }
  }

  removeMarker(index: number): void {
    if (!this.map || !this.markers[index]) return;

    // Remove marker from map
    this.map.removeLayer(this.markers[index]);
    
    // Remove from arrays
    this.markers.splice(index, 1);
    this.keyPoints.splice(index, 1);
    
    // Update selection
    if (this.selectedKeyPointIndex === index) {
      this.selectedKeyPointIndex = this.keyPoints.length > 0 ? 0 : null;
    } else if (this.selectedKeyPointIndex !== null && this.selectedKeyPointIndex > index) {
      this.selectedKeyPointIndex--;
    }

    // Recreate all markers with updated indices
    this.recreateAllMarkers();
    this.updatePolyline();
  }

  private recreateAllMarkers(): void {
    if (!this.map) return;

    // Clear existing markers
    this.markers.forEach(marker => {
      if (marker) this.map!.removeLayer(marker);
    });
    this.markers = [];

    // Recreate markers
    this.keyPoints.forEach((keyPoint, index) => {
      const position = L.latLng(keyPoint.latitude, keyPoint.longitude);
      this.createMarker(position, index);
    });
  }

  handleMarkerClick(index: number): void {
    this.selectedKeyPointIndex = index;
  }

  onInputChange(field: keyof KeyPoint, value: string): void {
    if (this.selectedKeyPointIndex === null || !this.keyPoints[this.selectedKeyPointIndex]) return;

    this.keyPoints[this.selectedKeyPointIndex] = {
      ...this.keyPoints[this.selectedKeyPointIndex],
      [field]: value
    };

    // Update popup if name changed
    if (field === 'name' && this.markers[this.selectedKeyPointIndex]) {
      this.updateMarkerPopup(this.markers[this.selectedKeyPointIndex], this.selectedKeyPointIndex);
    }
  }

  onFileChange(event: Event): void {
    if (this.selectedKeyPointIndex === null) return;

    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.keyPoints[this.selectedKeyPointIndex] = {
        ...this.keyPoints[this.selectedKeyPointIndex],
        pictureFile: input.files[0]
      };

      // Reset file input
      if (this.fileInput) {
        this.fileInput.nativeElement.value = '';
      }
    }
  }

  async finishTour(): Promise<void> {
    if (!this.tourId) {
      this.errorMessage = 'Tour ID not found';
      return;
    }

    for (let i = 0; i < this.keyPoints.length; i++) {
      const kp = this.keyPoints[i];
      if (!kp.name.trim() || !kp.description.trim()) {
        this.errorMessage = `Please fill in all required fields for Key Point ${i + 1}`;
        return;
      }
       if (!kp.pictureFile) {
        this.errorMessage = `Please upload a picture for Key Point ${i + 1}`;
        return;
      }
    }

    this.errorMessage = '';

    try {
      // Use the service method to upload all key points
      await this.tourService.addMultipleKeyPoints(this.tourId, this.keyPoints);
      
      // Navigate to tours page on success
      this.router.navigate(['/my-tours']);
      
    } catch (error) {
      console.error('Error uploading files:', error);
      this.errorMessage = 'Error uploading key points';
    }
  }
  

  get selectedKeyPoint(): KeyPoint | null {
    return this.selectedKeyPointIndex !== null && this.keyPoints[this.selectedKeyPointIndex]
      ? this.keyPoints[this.selectedKeyPointIndex] 
      : null;
  }
}