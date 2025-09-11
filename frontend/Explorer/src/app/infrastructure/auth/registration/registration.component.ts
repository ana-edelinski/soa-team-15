import { Component } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Registration } from '../model/registration.model';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'xp-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.css']
})
export class RegistrationComponent {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

 registrationForm = new FormGroup({
  username: new FormControl('', [Validators.required]),
  password: new FormControl('', [Validators.required]),
  email: new FormControl('', [Validators.required]),
  role: new FormControl('Tourist', [Validators.required]),  // default role
});


  register(): void {
  const registration: Registration = {
    username: this.registrationForm.value.username || "",
    password: this.registrationForm.value.password || "",
    email: this.registrationForm.value.email || "",
    role: this.registrationForm.value.role || "Tourist"
  };

  if (this.registrationForm.valid) {
    this.authService.register(registration).subscribe({
      next: () => {
        this.router.navigate(['home']);
      },
    });
  }
}

}
