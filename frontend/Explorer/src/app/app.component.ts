import { Component, OnInit } from '@angular/core';
import { AuthService } from './infrastructure/auth/auth.service';
import { User } from './infrastructure/auth/model/user.model';
import { Router } from '@angular/router';
import * as L from 'leaflet';
import markerIcon2x from 'leaflet/dist/images/marker-icon-2x.png';
import markerIcon   from 'leaflet/dist/images/marker-icon.png';
import markerShadow from 'leaflet/dist/images/marker-shadow.png';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'Explorer';
  isLoggedIn = false;
  currentUserRole: string;

  constructor(
    private authService: AuthService, private router: Router
  ) {}

  user: User | undefined;

  ngOnInit(): void {
    this.checkIfUserExists();
    this.authService.user$.subscribe(user => {
      this.user = user;
       this.isLoggedIn = !!user.username;
       this.currentUserRole = user.role;
    });

    //tera Leaflet da koristi ikone iz assets
    L.Icon.Default.mergeOptions({
      iconRetinaUrl: 'assets/leaflet/marker-icon-2x.png',
      iconUrl:       'assets/leaflet/marker-icon.png',
      shadowUrl:     'assets/leaflet/marker-shadow.png'
    });
  }

  logout() {
    this.authService.logout();
     this.router.navigate(['/login']);
  }
  
  private checkIfUserExists(): void {
    this.authService.checkIfUserExists();
  }
}
