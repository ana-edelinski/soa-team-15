import { Component, OnInit } from '@angular/core';
import { BlogService } from '../blog.service';
import { LayoutService } from 'src/app/feature-modules/layout/layout.service';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class ListComponent implements OnInit {
  blogs: any[] = [];
  authorUsernames: { [key: string]: string } = {};

  constructor(private blogService: BlogService, private profileService: LayoutService) {}

  ngOnInit(): void {
    this.blogService.getAllBlogs().subscribe({
    next: (data) => {
      this.blogs = data;

      const uniqueAuthorIds = [...new Set(this.blogs.map(b => b.authorId))];
      console.log("idevi"+uniqueAuthorIds);

      uniqueAuthorIds.forEach(id => {
        this.profileService.getProfile(id).subscribe(profile => {
          console.log(profile);
          this.authorUsernames[id] = profile.userName;
        });
      });
    },
    error: (err) => console.error('Greška pri dohvaćanju blogova', err)
  });
  
  }
}
