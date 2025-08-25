import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, Subject } from "rxjs";
import { Tour } from "./model/tour.model";
import { environment } from "src/env/environment";

@Injectable({
    providedIn: 'root'
  })
  export class TourService {
    private baseUrl = 'http://localhost:5226/api/';

    constructor(private http: HttpClient) { }
  
    getToursForAuthor(id: number): Observable<Tour[]> {
      return this.http.get<Tour[]>(`${this.baseUrl}author/tour/${id}`);
    }
  
    addTour(tour: Tour): Observable<Tour> {
      return this.http.post<Tour>(`${this.baseUrl}author/tour`, tour);
    }

  }