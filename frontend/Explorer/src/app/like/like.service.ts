import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LikeService {
  private API_BASE = 'http://localhost:8081/api/blogs';

  constructor(private http: HttpClient) {}

  toggleLike(blogId: string, authorId: number = 1): Observable<any> {
    return this.http.post(`${this.API_BASE}/${blogId}/like`, { authorId });
  }

  countLikes(blogId: string): Observable<{ likeCount: number }> {
    return this.http.get<{ likeCount: number }>(`${this.API_BASE}/${blogId}/like`);
  }

  isLikedByUser(blogId: string, authorId: number = 1): Observable<{ liked: boolean }> {
    return this.http.get<{ liked: boolean }>(`${this.API_BASE}/${blogId}/likedByMe?authorId=${authorId}`);
  }
}
