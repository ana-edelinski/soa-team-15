import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, map } from "rxjs";

@Injectable({ providedIn: 'root' })
export class FollowersService {
  private apiUrl = 'http://localhost:8083/api/follow';

  constructor(private http: HttpClient) {}

  follow(userId: string | null, targetId: string): Observable<void> {
    // Backend uzima userId iz JWT; body je prazan
    return this.http.post<void>(`${this.apiUrl}/${targetId}`, {});
  }

  unfollow(userId: string | null, targetId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${targetId}`, { body: {} });
  }

  isFollowing(userId: string | null, targetId: string): Observable<boolean> {
    return this.http
      .get<{following: boolean}>(`${this.apiUrl}/is-following`, { params: { u: String(userId), t: targetId } })
      .pipe(map(r => !!r?.following));
  }

  getRecommendations(userId: string | null): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/recommendations/${userId}`);
  }

  getFollowingOf(userId: string): Observable<string[]> {
  return this.http.get<string[]>(`${this.apiUrl}/following/${userId}`);
}

}
