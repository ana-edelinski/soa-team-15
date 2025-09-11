import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { TourReview, TourReviewCreate } from './model/tour-review.model';

@Injectable({ providedIn: 'root' })
export class TourReviewService {
  private baseUrl = 'http://localhost:8082/api';  

  constructor(private http: HttpClient) {}

  create(tourId: number, body: TourReviewCreate): Observable<TourReview> {
    return this.http.post<TourReview>(`${this.baseUrl}/tours/${tourId}/reviews`, body);
  }

  getByTour(tourId: number) {
    return this.http.get<TourReview[]>(`${this.baseUrl}/tours/${tourId}/reviews`);
  }

  summary(tourId: number) {
    return this.http.get<{ count: number; averageRating: number }>(
      `${this.baseUrl}/tours/${tourId}/reviews/summary`
    );
  }

  delete(tourId: number, reviewId: number) {
    return this.http.delete<void>(`${this.baseUrl}/tours/${tourId}/reviews/${reviewId}`);
  }

  
  uploadImages(files: File[]) {
    const fd = new FormData();
    files.forEach(f => fd.append('files', f));     // 👈 isti key 'files'
    return this.http.post<string[]>(`${this.baseUrl}/uploads/reviews`, fd);
  }

}
