import { Component, OnInit } from '@angular/core';
import { BlogService } from '../blog.service';
import { LayoutService } from 'src/app/feature-modules/layout/layout.service';
import { TokenStorage } from 'src/app/infrastructure/auth/jwt/token.service';
import { FollowersService } from '../followers.service';



@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.css']
})
export class ListComponent implements OnInit {
  blogs: any[] = [];
  authorUsernames: { [key: string]: string } = {};
  currentUserId: number | string | null = null;
  loading = false;
  following: { [key: string]: boolean } = {};

  slickConfig = {
    slidesToShow: 1,
    slidesToScroll: 1,
    autoplay: true,
    arrows: true, // Strelice za levo i desno
    autoplaySpeed: 3000,
    dots: false, // Da li želeš i dot navigaciju
    prevArrow: '<button type="button" class="slick-prev"> < </button>',
    nextArrow: '<button type="button" class="slick-next"> > </button>'
  };
  followingReady = false; // kada završimo učitavanje koga pratiš

private k(id: any): string { return String(id); }

private isFollowing(authorId: any): boolean {
  return !!this.following[this.k(authorId)];
}


  constructor(
    private blogService: BlogService,
    private profileService: LayoutService,
    private tokenStorage: TokenStorage,
    private followersService : FollowersService
  ) {}

  ngOnInit(): void {
    this.extractCurrentUserIdFromJwt();
    this.loadBlogs();
  }

  private extractCurrentUserIdFromJwt(): void {
    const token = this.tokenStorage.getAccessToken?.();
    if (!token) {
      this.currentUserId = null;
      return;
    }
    try {
      const payloadBase64 = token.split('.')[1]
        .replace(/-/g, '+')
        .replace(/_/g, '/');
      const json = JSON.parse(atob(payloadBase64));
      // pokrivamo razne tipične ključeve
      const candidates = ['id', 'userId', 'nameid', 'sub'];
      for (const k of candidates) {
        if (json?.[k] !== undefined && json?.[k] !== null) {
          this.currentUserId = json[k];
          break;
        }
      }
    } catch {
      this.currentUserId = null;
    }
  }

  loadBlogs(): void {
    this.loading = true;
    this.blogService.getAllBlogs().subscribe({
      next: (data) => {
        this.blogs = (data ?? []).sort((a, b) => {
          const da = new Date(a?.createdAt ?? 0).getTime();
          const db = new Date(b?.createdAt ?? 0).getTime();
          return db - da; // najnoviji prvi
        });

        const uniqueAuthorIds = [...new Set(this.blogs.map(b => b.authorId))];
        uniqueAuthorIds.forEach((id: any) => {
          this.profileService.getProfile(id).subscribe({
            next: (profile: any) => (this.authorUsernames[id] = profile?.userName ?? ''),
            error: () => (this.authorUsernames[id] = '')
          });
        });


 // (2) Učitaj koga pratim (jedan backend poziv) i popuni mapu
      const uid = this.currentUserId ? String(this.currentUserId) : null;
     
        if (!uid) {
          // nije ulogovan → nema komentarisanja
          this.followingReady = true;   // <<< DODATO
        } else {
          this.followersService.getFollowingOf(uid).subscribe({
            next: (list) => {
              (list ?? []).forEach(id => this.following[String(id)] = true);
              // autor može da komentariše svoj blog — to rešavamo u canComment()
              this.followingReady = true;  // <<< DODATO
            },
            error: () => {
              this.followingReady = true;  // <<< DODATO (da UI ne visi)
            }
          });
        }

      },
      error: (err) => console.error('Greška pri dohvaćanju blogova', err),
      complete: () => (this.loading = false)
    });
  }

  getAuthorLabel(authorId: any): string {
    const uname = this.authorUsernames?.[authorId];
    if (uname && uname.trim().length > 0) return `@${uname}`;
    return `Korisnik #${authorId}`;
  }

  canDelete(blog: any): boolean {
    if (this.currentUserId === null || this.currentUserId === undefined) return false;
    return String(blog?.authorId) === String(this.currentUserId);
  }

  onDelete(id: string): void {
    if (!id) return;
    const ok = confirm('Da li sigurno želite da obrišete ovaj blog?');
    if (!ok) return;

    this.blogService.deleteBlog(id).subscribe({
      next: () => this.loadBlogs(),
      error: (err) => console.error('Greška pri brisanju bloga', err)
    });
  }
toggleFollow(authorId: string): void {
  const userId = this.currentUserId ? String(this.currentUserId) : null;

  if (this.following[authorId]) {
    this.followersService.unfollow(userId, authorId).subscribe({
      next: () => {
        this.following[authorId] = false;
        console.log(`User ${userId} UNFOLLOWED author ${authorId}`);
        alert('Unfollowed!');
      },
      error: (err) => {
        console.error('Error unfollowing user:', err);
        alert('Error unfollowing user');
      }
    });
  } else {
    this.followersService.follow(userId, authorId).subscribe({
      next: () => {
        this.following[authorId] = true;
        console.log(`User ${userId} FOLLOWED author ${authorId}`);
        alert('Followed!');
      },
      error: (err) => {
        console.error('Error following user:', err);
        alert('Error following user');
      }
    });
  }
}

canComment(authorId: string | number): boolean {
  if (this.currentUserId == null) return false; // mora biti ulogovan
  if (String(this.currentUserId) === String(authorId)) return true; // autor uvek može
  return this.isFollowing(authorId); // inače samo ako ga prati
}


}
