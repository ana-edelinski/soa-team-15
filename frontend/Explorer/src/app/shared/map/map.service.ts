import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { map, shareReplay } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MapService {

  private cache = new Map<string, Observable<any>>();

  constructor(private http: HttpClient) {}

  reverse(lat: number, lon: number): Observable<any> {
    const key = `${lat.toFixed(6)},${lon.toFixed(6)}`;
    const cached = this.cache.get(key);
    if (cached) return cached;

    // Nominatim traži pristojan User-Agent / Referer
    const headers = new HttpHeaders({
      'Accept': 'application/json',
      'User-Agent': 'ExplorerApp/1.0 (dev@example.com)'
    });

    const req$ = this.http
      .get(`https://nominatim.openstreetmap.org/reverse`, {
        headers,
        params: {
          format: 'json',
          lat: lat.toString(),
          lon: lon.toString(),
          addressdetails: '1'
        }
      })
      .pipe(shareReplay(1)); // keširaj rezultat za iste koordinate

    this.cache.set(key, req$);
    return req$;
  }
}
