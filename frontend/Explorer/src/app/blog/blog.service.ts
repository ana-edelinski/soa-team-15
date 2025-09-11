import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class BlogService {
  private apiUrl = 'http://localhost:8081/api/blogs';

  constructor(private http: HttpClient) {}

  getAllBlogs(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }
}
