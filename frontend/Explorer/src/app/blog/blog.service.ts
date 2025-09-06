import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class BlogService {
  private apiUrl = 'http://localhost:8090/api/blogs';

  constructor(private http: HttpClient) {}

  getAllBlogs(): Observable<{ blogs: any[] }> {
    return this.http.get<{ blogs: any[] }>(this.apiUrl);
  }

}
