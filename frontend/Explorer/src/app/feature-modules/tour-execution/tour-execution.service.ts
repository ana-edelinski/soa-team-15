import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { TourExecution, TourForTourist } from './tour-execution.model';
import { catchError, Observable, of, throwError } from 'rxjs';
import { environment } from 'src/env/environment';
import { Position } from './tour-execution.model';
import { Tour } from '../tour-authoring/model/tour.model';

@Injectable({
  providedIn: 'root'
})
export class TourExecutionService {
  
  private apiUrl = 'http://localhost:8082/api/position/'; //8082
  private baseUrl = 'http://localhost:8082/api/execution';

  constructor(private http: HttpClient) { }

   updateMyPosition(lat: number, lng: number, touristId: number) {

    return this.http.put(`${this.apiUrl}`, { latitude: lat, longitude: lng, touristId: touristId});
  }

  getMyPosition(userId: number) {
  return this.http.get<Position>(this.apiUrl+userId).pipe(
      catchError(err => err.status === 404 ? of(null) : throwError(() => err))
    );
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

  getActiveExecutionForTourist(touristId: number) {
    return this.http.get<TourExecution>(`${this.baseUrl}/active/${touristId}`).pipe(
      catchError(err => err.status === 404 ? of(null) : throwError(() => err))
    );
  }

  getStartableToursForUser(id: number): Observable<Tour[]> { // za sada dobavlja publikovane posle kupljene
    return this.http.get<Tour[]>('http://localhost:8082/api/tour/published');
  }

  getTourInfoForExecution(id: number): Observable<TourForTourist> {
    return this.http.get<TourForTourist>('http://localhost:8082/api/tour/'+id);
  }

}
