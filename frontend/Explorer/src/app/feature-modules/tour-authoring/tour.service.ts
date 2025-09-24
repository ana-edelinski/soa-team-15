import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, Subject } from "rxjs";
import { Tour } from "./model/tour.model";
import { environment } from "src/env/environment";
import { KeyPoint } from "./model/keypoint.model";
import { PublicTour } from './model/tour-public.model'; // ⬅ importaj model
import { firstValueFrom } from 'rxjs';
import { ToursResponse } from "./model/tours-proto-response.model";


export enum TransportType { Walk = 0, Bike = 1, Car = 2 }
export interface TransportTimeDto {
  id?: number;        
  tourId?: string;    
  type: TransportType; 
  minutes: number;
}





@Injectable({
    providedIn: 'root'
  })
  export class TourService {
    private baseUrl = 'http://localhost:8082/api/';
    private gatewayUrl = 'http://localhost:8090/api/';

    constructor(private http: HttpClient) { }
  
    getToursForAuthor(id: number): Observable<ToursResponse> {
      return this.http.get<ToursResponse>(`${this.gatewayUrl}author/tour/${id}`);
    }
  
    addTour(tour: Tour): Observable<Tour> {
      return this.http.post<Tour>(`${this.gatewayUrl}author/tour`, tour);
    }
    getPublishedTours(): Observable<Tour[]> {
      return this.http.get<Tour[]>(`${this.baseUrl}tour/published`);
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
      formData.append('image', ' a');
    
      return this.http.post(
        `${this.baseUrl}author/tour/addKeyPoints/${tourId}`,
        formData
      );
    }

//    async addMultipleKeyPoints(tourId: string, keyPoints: KeyPoint[]): Promise<boolean> {
  //    try {
    //    for (const keyPoint of keyPoints) {
      //    const response = await this.addKeyPoint(tourId, keyPoint).toPromise();
          
          
        //}
       // return true;
     // } catch (error) {
      //  console.error('Error uploading key points:', error);
       // throw error;
      //}
   // }


    async addMultipleKeyPoints(tourId: string, keyPoints: KeyPoint[]): Promise<boolean> {
      try {
        for (const keyPoint of keyPoints) {
          await firstValueFrom(this.addKeyPoint(tourId, keyPoint));
        }
        return true;
      } catch (error) {
        console.error('Error uploading key points:', error);
        throw error;
      }
    }

    
      async updateTourKM(tourId: string): Promise<number> {
        const url = `${this.baseUrl}author/tour/updateTourKm/${tourId}`;
        // Ako si backend prebacio da računa iz baze i NE prima body, šalji prazan objekat:
        const res = await firstValueFrom(this.http.post<number>(url, {}));
        return res ?? 0;
      }

     getKeyPointsForTour(tourId: string): Observable<KeyPoint[]> {
        return this.http.get<KeyPoint[]>(`${this.baseUrl}author/tour/getKeyPoints/${tourId}`);
      }


      
      getTourById(tourId: string) {
        return this.http.get<Tour>(`${this.baseUrl}author/tour/byId/${tourId}`);
      }
      
      publishTour(tourId: string) {
        return this.http.post<void>(`${this.baseUrl}author/tour/${tourId}/publish`, {});
      }
      archiveTour(tourId: string) {
        return this.http.post<void>(`${this.baseUrl}author/tour/${tourId}/archive`, {});
      }
      reactivateTour(tourId: string) {
        return this.http.post<void>(`${this.baseUrl}author/tour/${tourId}/reactivate`, {});
      }
      




      createTransportTime(tourId: string | number, type: number, minutes: number) {
        const url = `${this.baseUrl}author/tour/${tourId}/transport-times`;
        const body = { type, minutes };
        return this.http.post<void>(url, body);
      }

      // UPDATE jedan transport time
      updateTransportTime(tourId: string | number, type: number, minutes: number) {
        const url = `${this.baseUrl}author/tour/${tourId}/transport-times`;
        const body = { type, minutes };
        return this.http.put<void>(url, body);
      }

      // (opciono) lokalni proračun minuta kao na backendu (5 / 16 / 50 km/h)
      calcMinutes(distanceKm: number, type: number): number {
        const speed = type === 0 ? 5.0 : type === 1 ? 16.0 : 50.0;
        if (distanceKm <= 0 || speed <= 0) return 0;
        return Math.round((distanceKm / speed) * 60.0);
      }


      getTransportTimes(tourId: string | number) {
        const url = `${this.baseUrl}author/tour/${tourId}/transport-times`;
        return this.http.get<TransportTimeDto[]>(url);
      }


      getToursByUserId(userId: number): Observable<Tour[]> {
        return this.http.get<Tour[]>(`${this.baseUrl}/${userId}`);
      }

      getAllIncludingUnpublished(): Observable<Tour[]> {
        return this.http.get<Tour[]>(`${this.baseUrl}author/tour/all-with-unpublished`); // ✅
      }




      getPublicTours() { return this.http.get<PublicTour[]>(`${this.baseUrl}public/tours`); }
      getPublicTourById(id: number | string) { return this.http.get<PublicTour>(`${this.baseUrl}public/tours/${id}`); }


  }