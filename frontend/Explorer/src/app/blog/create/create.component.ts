import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';

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

  constructor(private http: HttpClient) {}

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
    if (!this.blog.title || !this.blog.content || !this.blog.authorId) {
      alert("Please fill in all required fields!");
      return;
    }

    const formData = new FormData();
    formData.append('title', this.blog.title);
    formData.append('content', this.blog.content);
    formData.append('authorId', this.blog.authorId);

    // Append all selected images
    for (let i = 0; i < this.selectedImages.length; i++) {
      formData.append('images', this.selectedImages[i]);
    }

    console.log('Uploading blog with', this.selectedImages.length, 'images');

    this.http.post('http://localhost:8081/api/blogs', formData).subscribe({
      next: (response) => {
        console.log('Blog created successfully:', response);
        alert('Blog successfully created!');
        // Reset form
        this.blog = { title: '', content: '', authorId: '', date: '' };
        this.selectedImages = [];
        this.imagePreviews = [];
      },
      error: (err) => {
        console.error('Error creating blog:', err);
        alert('Error: ' + (err.error || err.message));
      }
    });
  }
}