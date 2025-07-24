import { Component } from '@angular/core';
import { LayoutService } from '../layout.service';
import { Profile } from 'src/app/infrastructure/auth/model/profile.model';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';

@Component({
  selector: 'xp-profile-info',
  templateUrl: './profile-info.component.html',
  styleUrls: ['./profile-info.component.css']
})
export class ProfileInfoComponent {

  infoPerson: Profile; // originalni podaci
  editPerson: Profile; // podaci koji se edituju
  editMode = false;
  imageBase64: string | null = null;
  currentUser: User |null;

  constructor(private layoutService: LayoutService, private authService: AuthService) {}

  ngOnInit(): void {
    this.loadProfile();
  }
  loadProfile(): void {
    const currentUser = this.authService.user$.getValue();
    this.layoutService.getProfile(currentUser.id).subscribe((data: any) => {
      this.infoPerson = data;
      console.log(data);

    });
  }

  enableEditMode(): void {
    this.editMode = true;
     this.editPerson = { ...this.infoPerson };
  }

  cancelEdit(): void {
    this.editMode = false;
    this.imageBase64 = null;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const reader = new FileReader();
      reader.onload = () => {
        this.imageBase64 = reader.result as string;
        this.editPerson.ImageBase64 = reader.result as string;
      };
      reader.readAsDataURL(input.files[0]);
    }
  }

  updateProfile(): void {
    // Slanje podataka ka backendu (zajedno sa novom slikom ako postoji)
    
    const updated = {
      ...this.editPerson,
    };
    console.log(updated);
    this.layoutService.updateProfile(updated).subscribe((response: any) => {
      this.infoPerson = response; 
      this.loadProfile();
      this.cancelEdit();
    });
  }

  getImage(imagePath: string): string {
    return imagePath
    ? `http://localhost:8080/images/${imagePath}`
    : '/assets/images/default.png';
  }

}
