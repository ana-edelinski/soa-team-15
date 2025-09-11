import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, Subject } from "rxjs";
import { Tour } from "./model/tour.model";
import { environment } from "src/env/environment";
import { KeyPoint } from "./model/keypoint.model";

@Injectable({
    providedIn: 'root'
  })
  export class TourService {
    private baseUrl = 'http://localhost:8082/api/';

    constructor(private http: HttpClient) { }
  
    getToursForAuthor(id: number): Observable<Tour[]> {
      return this.http.get<Tour[]>(`${this.baseUrl}author/tour/${id}`);
    }
  
    addTour(tour: Tour): Observable<Tour> {
      return this.http.post<Tour>(`${this.baseUrl}author/tour`, tour);
    }
      // NOVO: objavljene ture za browse (turista)
    getPublishedTours(): Observable<Tour[]> {
      return this.http.get<Tour[]>(`${this.baseUrl}author/tour/published`);
    }


    addKeyPoint(tourId: string, keyPoint: KeyPoint): Observable<any> {
      const formData = new FormData();
      
      if (keyPoint.pictureFile) {
        formData.append('pictureFile', keyPoint.pictureFile);
      }
      formData.append('latitude', keyPoint.latitude.toString());
      formData.append('longitude', keyPoint.longitude.toString());
      formData.append('name', keyPoint.name);
      formData.append('description', keyPoint.description);

    
      return this.http.post(
        `${this.baseUrl}author/tour/addKeyPoints/${tourId}`,
        formData
      );
    }

    // Method to add multiple key points
    async addMultipleKeyPoints(tourId: string, keyPoints: KeyPoint[]): Promise<boolean> {
      try {
        for (const keyPoint of keyPoints) {
          const response = await this.addKeyPoint(tourId, keyPoint).toPromise();
          
          
        }
        return true;
      } catch (error) {
        console.error('Error uploading key points:', error);
        throw error;
      }
    }

     getKeyPointsForTour(tourId: string): Observable<KeyPoint[]> {
      return this.http.get<KeyPoint[]>(`${this.baseUrl}author/tour/getKeyPoints/${tourId}`);
    }

  }