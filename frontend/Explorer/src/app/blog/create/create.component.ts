import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.css']
})
export class CreateComponent {
  blog = {
    title: '',
    content: '',
    authorId: '',
    date: ''
  };

  selectedImages: File[] = [];
  imagePreviews: string[] = [];
  user: User;

  constructor(private http: HttpClient, private authService: AuthService) {}


  ngOnInit(): void {
    const currentUser = this.authService.user$.getValue();
    this.user = currentUser;
    console.log(this.user);
  }

  onImageSelected(event: any) {
  const files: FileList = event.target.files;

  for (let i = 0; i < files.length; i++) {
    const file = files[i];

    // Avoid duplicates based on file name (optional)
    if (this.selectedImages.find(f => f.name === file.name && f.size === file.size)) {
      continue;
    }

    this.selectedImages.push(file);

    // Create preview
    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.imagePreviews.push(e.target.result);
    };
    reader.readAsDataURL(file);
  }
}


  removeImage(index: number) {
    this.selectedImages.splice(index, 1);
    this.imagePreviews.splice(index, 1);
  }

 createBlog() {
  console.log('🔄 Form submitted');
  console.log('📦 Blog:', this.blog);

  this.blog.authorId = this.user.id.toString();

  // Validacija: sva polja osim slika moraju biti popunjena
  if (!this.blog.title || !this.blog.content || !this.blog.authorId) {
    alert("Please fill in all required fields!");
    return;
  }

  const formData = new FormData();

  // Obavezna polja
  formData.append('title', this.blog.title);
  formData.append('content', this.blog.content);
  formData.append('authorId', this.blog.authorId);

  // Datum – automatski postavljen u ISO formatu (npr. 2025-07-22T19:15:00Z)
  formData.append('date', new Date().toISOString());

  // Slike (opciono)
  for (let i = 0; i < this.selectedImages.length; i++) {
    formData.append('images', this.selectedImages[i]);
  }

  // Loguj pre slanja
  console.log('📤 Uploading blog with', this.selectedImages.length, 'images');
  console.log('✅ Sending POST to http://localhost:8081/api/blogs');

  // HTTP poziv
  this.http.post('http://localhost:8081/api/blogs', formData).subscribe({
    next: (response) => {
      console.log('✅ Blog created successfully:', response);
      alert('Blog successfully created!');

      // Reset forme
      this.blog = { title: '', content: '', authorId: '', date: '' };
      this.selectedImages = [];
      this.imagePreviews = [];
    },
    error: (err) => {
      console.error('❌ Error creating blog:', err);
      alert('Error: ' + (err.error?.message || err.message || 'Unknown error'));
    }
  });
}

}