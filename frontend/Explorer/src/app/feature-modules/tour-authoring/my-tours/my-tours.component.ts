import { Component, OnInit } from '@angular/core';
import { TourService } from '../tour.service';
import { Tour } from '../model/tour.model';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { TourStatus } from '../model/tour-status.model';

@Component({
  selector: 'xp-my-tours',
  templateUrl: './my-tours.component.html',
  styleUrls: ['./my-tours.component.css']
})
export class MyToursComponent implements OnInit {

  tours: Tour[] = [];
  user: User | null = null;
  error_message = '';

  constructor(private tourService: TourService, private authService: AuthService) {
    this.authService.user$.subscribe(u => this.user = u);
  }

  ngOnInit(): void {
    if (this.user) {
      this.tourService.getToursForAuthor(this.user.id).subscribe({
        next: (data: any) => {
          console.log("Tours from API:", data);
          this.tours = data.tours;   
        },
        error: (err) => {
          console.error(err);
          this.error_message = 'Failed to load tours.';
        }
      });
    }
  }

  getStatusLabel(status: number): string {
    return TourStatus[status];
  }
}
