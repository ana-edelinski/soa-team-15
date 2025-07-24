import { Component, OnInit } from '@angular/core';
import { AuthService } from './infrastructure/auth/auth.service';
import { User } from './infrastructure/auth/model/user.model';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'Explorer';

  constructor(
    private authService: AuthService,
  ) {}

  user: User | undefined;

  ngOnInit(): void {
    this.checkIfUserExists();
    this.authService.user$.subscribe(user => {
      this.user = user;
      console.log("Navbar:" +user)
    });
  }
  
  private checkIfUserExists(): void {
    this.authService.checkIfUserExists();
  }
}
