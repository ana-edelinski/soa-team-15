import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { TourExecution } from './tour-execution.model';
import { Observable } from 'rxjs';
import { environment } from 'src/env/environment';

@Injectable({
  providedIn: 'root'
})
export class TourExecutionService {

  private apiUrl = 'http://localhost:5226/api/position/'; //8082
  private baseUrl = 'http://localhost:5226/api/execution';

  constructor(private http: HttpClient) { }

   updateMyPosition(lat: number, lng: number, touristId: number) {

    return this.http.put(`${this.apiUrl}`, { latitude: lat, longitude: lng, touristId: touristId});
  }

  // ako ti zatreba da učitaš poslednju:
  getMyPosition(userId: number) {
    return this.http.get<{ latitude: number; longitude: number }>(this.apiUrl + userId );
  }

  startTourExecution(execution: TourExecution): Observable<TourExecution> {
    return this.http.post<TourExecution>(this.baseUrl, execution);
  }

  completeTourExecution(id: number): Observable<TourExecution> {
    return this.http.post<TourExecution>(`${this.baseUrl}/complete/${id}`, {});
  }

  abandonTourExecution(id: number): Observable<TourExecution> {
    return this.http.post<TourExecution>(`${this.baseUrl}/abandon/${id}`, {});
  }

  getExecutionByTourAndTourist(touristId: number, tourId: number): Observable<TourExecution> {
    return this.http.get<TourExecution>(`${this.baseUrl}/by_tour_and_tourist/${touristId}/${tourId}`);
  }

  getActiveExecutionForTourist(touristId: number): Observable<TourExecution> {
    return this.http.get<TourExecution>(`${this.baseUrl}/active/${touristId}`);
  }
}
