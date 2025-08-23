import { Component, OnInit } from '@angular/core';
import { AuthService } from './infrastructure/auth/auth.service';
import { User } from './infrastructure/auth/model/user.model';
import { Router } from '@angular/router';

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
  }

  logout() {
    this.authService.logout();
     this.router.navigate(['/login']);
  }
  
  private checkIfUserExists(): void {
    this.authService.checkIfUserExists();
  }
}
