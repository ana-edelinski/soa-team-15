import { Component, Input, OnInit } from '@angular/core';
import { LikeService } from '../like/like.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'xp-like',
  templateUrl: './like.component.html',
  styleUrls: ['./like.component.css']
})
export class LikeComponent implements OnInit {

  @Input() blogId!: string;
  likeCount: number = 0;
  liked: boolean = false;
  authorId: number = 1; 

  userId: number = 1;

  constructor(private http: HttpClient, private likeService: LikeService) {}

  ngOnInit(): void {
      console.log("INIT blogId =", this.blogId); // Dodaj za proveru

    this.loadLikeState();
  }

  loadLikeState(): void {
        //  console.log("📌 blogId:", this.blogId); // Dodaj ovo

    this.likeService.countLikes(this.blogId).subscribe(res => {
      this.likeCount = res.likeCount;
    });

    this.likeService.isLikedByUser(this.blogId, this.authorId).subscribe(res => {
      this.liked = res.liked;
    });
  }

  toggleLike(): void {
  console.log("📌 CLICKED!", this.blogId, this.authorId); // Dodaj

    this.likeService.toggleLike(this.blogId, this.authorId).subscribe(() => {
      this.liked = !this.liked;
      this.likeCount += this.liked ? 1 : -1;
    });
  }
}