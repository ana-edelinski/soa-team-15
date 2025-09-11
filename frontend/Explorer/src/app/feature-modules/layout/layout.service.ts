import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/env/environment';
import { Observable } from 'rxjs';
import { PagedResults } from 'src/app/shared/model/paged-results.model';
import { Account } from 'src/app/infrastructure/auth/model/account.model';
import { Profile } from 'src/app/infrastructure/auth/model/profile.model';
import { User } from 'src/app/infrastructure/auth/model/user.model';

@Injectable({
  providedIn: 'root'
})
export class LayoutService {

  constructor(private http: HttpClient) { }

  getProfile(id: number): Observable<Profile> {
    return this.http.get<Profile>(environment.apiHost + 'profile/'+id);
  }
 

  updateProfile(user: Profile): Observable<Profile> {
    return this.http.put<Profile>(environment.apiHost + 'profile', user);
  }



}