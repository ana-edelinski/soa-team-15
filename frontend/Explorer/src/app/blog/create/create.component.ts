import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { forkJoin, map, Observable } from 'rxjs';

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

uploadImage(file: File): Observable<string> {
  return new Observable(observer => {
    const reader = new FileReader();
    reader.onload = () => {
      if (!reader.result) {
        observer.error("FileReader result is empty");
        return;
      }

      const base64 = (reader.result as string).split(',')[1]; // samo base64 deo
      if (!base64) {
        observer.error("Base64 encoding failed");
        return;
      }

      this.http.post<{ url: string }>(
        'http://localhost:8090/api/blogs/upload',
        { file: base64, filename: file.name }
      ).subscribe({
        next: res => {
          observer.next(res.url);
          observer.complete();
        },
        error: err => observer.error(err)
      });
    };
    reader.readAsDataURL(file);
  });
}




createBlog() {
  console.log("📤 Starting blog creation...");

  // 1. Uploaduj sve slike kroz gRPC-Gateway
  forkJoin(this.selectedImages.map(img => this.uploadImage(img))).subscribe({
    next: (urls: string[]) => {
      console.log("✅ Uploaded images, URLs:", urls);

      // 2. Kreiraj blog sa dobijenim URL-ovima
      const blog = {
        title: this.blog.title,
        content: this.blog.content,
        authorId: this.user.id,  // pošto je number, backend ga prima normalno
        imagePaths: urls
      };

      this.http.post('http://localhost:8090/api/blogs', blog).subscribe({
        next: (res) => {
          console.log("✅ Blog created successfully:", res);
          alert("Blog created!");

          // reset forme
          this.blog = { title: '', content: '', authorId: '', date: '' };
          this.selectedImages = [];
          this.imagePreviews = [];
        },
        error: (err) => {
          console.error("❌ Error creating blog:", err);
          alert("Error creating blog: " + (err.error?.message || err.message));
        }
      });
    },
    error: (err) => {
      console.error("❌ Error uploading images:", err);
      alert("Error uploading images: " + (err.error?.message || err.message));
    }
  });
}



}