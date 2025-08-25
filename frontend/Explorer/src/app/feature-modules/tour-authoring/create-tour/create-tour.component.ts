import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { Tour } from '../model/tour.model';
import { TourService } from '../tour.service';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { Router } from '@angular/router';

export enum TourTags {
  Cycling = 0,
  Culture = 1,
  Adventure = 2,
  FamilyFriendly = 3,
  Nature = 4,
  CityTour = 5,
  Historical = 6,
  Relaxation = 7,
  Wildlife = 8,
  NightTour = 9,
  Beach = 10,
  Mountains = 11,
  Photography = 12,
  Guided = 13,
  SelfGuided = 14,
}

@Component({
  selector: 'xp-create-tour',
  templateUrl: './create-tour.component.html',
  styleUrls: ['./create-tour.component.css']
})
export class CreateTourComponent  implements OnChanges{

  @Output()tourUpdated = new EventEmitter<null>();
  @Input() tour: Tour;
  user: User | null = null;
  tourTags = Object.keys(TourTags)
  .filter(key => isNaN(Number(key)))
  .map((tag, index) => ({ index, label: tag }));
  error_message = '';
  currentTags: number[] = [];

  constructor(private service: TourService, private authService: AuthService, private router: Router) {
    this.authService.user$.subscribe((user) => {
      this.user = user; 
      console.log(user);

      
    });
  }

  ngOnChanges(): void {
    this.tourForm.reset();
    this.error_message = '';
  }

  tourForm = new FormGroup({
    name: new FormControl('', [Validators.required]),
    difficulty: new FormControl('', [Validators.required]),
    description: new FormControl('', [Validators.required]),
    tags: new FormControl<number[]>([]),
    price: new FormControl(0, [
      Validators.required,
      Validators.min(0.01) // Optional: Ensure price is greater than 0
    ])
    
  });

  addTour(): void {
    console.log(this.tourForm.value.price);
    if(this.user!== null && this.user?.role === 'TourAuthor'&& this.tourForm.value.price!>0 && this.tourForm.value.name!=='' && this.tourForm.value.description!==''&& this.tourForm.value.difficulty!=='')
    {
      if(this.currentTags.length === 0)
      {
        this.error_message = 'Please select at least one tag.';
      }
      else {
      
      console.log(this.tourForm.value.tags);
      const tour: Tour = {
        name: this.tourForm.value.name || "",
        description: this.tourForm.value.description || "",
        difficulty: this.tourForm.value.difficulty || "",
        tags: this.tourForm.value.tags || [],
        status: 0,
        price: Number(this.tourForm.value.price) || 0,
        userId: this.user.id
      };

    console.log(tour);

    this.service.addTour(tour).subscribe({
      next: () => { this.tourUpdated.emit();
        this.router.navigate(['/blogs']);  // /author/tours
       }
    });
  }} }
  
  onTagChange(event: MatCheckboxChange, index: number): void {
    this.currentTags = this.tourForm.get('tags')?.value || [];

    if (event.checked) {
      this.currentTags.push(index);
    } else {

      const tagIndex = this.currentTags.indexOf(index);
      if (tagIndex >= 0) {
        this.currentTags.splice(tagIndex, 1);
      }
    }
    this.tourForm.get('tags')?.setValue(this.currentTags);

  }
}

