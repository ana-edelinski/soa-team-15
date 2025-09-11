import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TourExecutionService {

  private apiUrl = 'http://localhost:5226/api/position/';

  constructor(private http: HttpClient) { }

   updateMyPosition(lat: number, lng: number, touristId: number) {

    return this.http.put(`${this.apiUrl}`, { latitude: lat, longitude: lng, touristId: touristId});
  }

  // ako ti zatreba da učitaš poslednju:
  getMyPosition(userId: number) {
    return this.http.get<{ latitude: number; longitude: number }>(this.apiUrl + userId );
  }
}
