import { Component,Input } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { v4 as uuidv4 } from 'uuid'; 

@Component({
  selector: 'xp-comment',
  templateUrl: './comment.component.html',
  styleUrls: ['./comment.component.css']
})
export class CommentComponent {
  @Input() blogId!: string;

  comments: any[] = [];
  commentText: string = '';
  authorId: number = 1; 

  constructor(private http: HttpClient) {}

  ngOnInit(): void{
    this.loadComments();
  }

  loadComments() {
    this.http.get<any[]>(`http://localhost:8081/api/blogs/${this.blogId}/comments`)
      .subscribe({
        next: (res) => {
          this.comments = res.reverse(); // Najnoviji prvi
        },
        error: (err) => console.error('❌ Greška pri dobavljanju komentara:', err)
      });
  }

  postComment() {

      console.log("📌 blogId:", this.blogId); // Dodaj ovo


    if (!this.commentText.trim()) return;

    const body = {
      authorID: this.authorId,
      blogId: this.blogId,
      content: this.commentText
    };

    this.http.post('http://localhost:8081/api/blogs/' + this.blogId + '/comments', body)
      .subscribe({
        next: (res) => {
          console.log('✅ Komentar uspešno poslat!', res);
          this.commentText = '';
                  this.comments.unshift(res);

        },
        error: (err) => {
          console.error('❌ Greška prilikom slanja komentara:', err);
        }
      });
  }
}