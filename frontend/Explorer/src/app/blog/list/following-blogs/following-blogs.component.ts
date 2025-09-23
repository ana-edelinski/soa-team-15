import { Component, OnInit } from '@angular/core';
import { LayoutService } from 'src/app/feature-modules/layout/layout.service';
import { TokenStorage } from 'src/app/infrastructure/auth/jwt/token.service';
import { BlogService } from '../../blog.service';
import { FollowersService } from '../../followers.service';


@Component({
  selector: 'app-following-blogs',
  templateUrl: './following-blogs.component.html',
  styleUrls: ['./following-blogs.component.css']
})
export class FollowingBlogsComponent implements OnInit {
  blogs: any[] = [];
  filtered: any[] = [];                        // 👈 samo blogovi autora koje pratim (+ moji)
  authorUsernames: Record<string, string> = {};
  currentUserId: string | null = null;
  loading = false;

  following: Record<string, boolean> = {};
  followingReady = false;

  constructor(
    private blogService: BlogService,
    private profileService: LayoutService,
    private tokenStorage: TokenStorage,
    private followersService: FollowersService
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.extractCurrentUserIdFromJwt();
    this.loadData();
  }

  private extractCurrentUserIdFromJwt(): string | null {
    const token = this.tokenStorage.getAccessToken?.();
    if (!token) return null;
    try {
      const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = JSON.parse(atob(payload));
      for (const k of ['id', 'userId', 'nameid', 'sub']) {
        if (json?.[k] != null) return String(json[k]);
      }
    } catch {}
    return null;
  }

  private loadData(): void {
    this.loading = true;

    // 1) Učitaj sve blogove (pa ćemo filtrirati)
    this.blogService.getAllBlogs().subscribe({
      next: (data) => {
        this.blogs = (data ?? []).sort((a: any, b: any) =>
          new Date(b?.createdAt ?? 0).getTime() - new Date(a?.createdAt ?? 0).getTime()
        );

        // povuci imena autora (samo jednom po id-u)
        const ids = Array.from(new Set(this.blogs.map(b => String(b.authorId))));
        ids.forEach(id => {
          const numericId = Number(id);
          this.profileService.getProfile(numericId).subscribe({
            next: (p: any) => this.authorUsernames[id] = p?.userName ?? '',
            error: () => this.authorUsernames[id] = ''
          });
        });

        // 2) Učitaj koga JA pratim pa filtriraj
        const uid = this.currentUserId;
        if (!uid) {
          // nije ulogovan → nema šta da prikažemo
          this.followingReady = true;
          this.filtered = [];
          this.loading = false;
          return;
        }

        this.followersService.getFollowingOf(uid).subscribe({
          next: (list) => {
            // obeleži praćene
            (list ?? []).forEach((fid: any) => this.following[String(fid)] = true);
            // u filtered stavi:
            //  - sve blogove autora koje pratim
            //  - moje sopstvene blogove (ako želiš)
            //const isMine = (a: any) => String(a.authorId) === uid;
            const isFollowed = (a: any) => !!this.following[String(a.authorId)];
            this.filtered = this.blogs.filter( isFollowed);

            this.followingReady = true;
            this.loading = false;
          },
          error: () => {
            // u slučaju greške ništa ne prikazujemo
            this.followingReady = true;
            this.filtered = [];
            this.loading = false;
          }
        });
      },
      error: () => { this.loading = false; }
    });
  }

  getAuthorLabel(authorId: any): string {
    const id = String(authorId);
    const uname = this.authorUsernames[id];
    return uname && uname.trim().length > 0 ? `@${uname}` : `Korisnik #${id}`;
  }

  // (opciono) Toggle follow/unfollow i live osvežavanje liste:
  toggleFollow(authorId: string): void {
    if (!this.currentUserId) return;
    const uid = this.currentUserId;
    const key = String(authorId);

    if (this.following[key]) {
      this.followersService.unfollow(uid, key).subscribe({
        next: () => {
          this.following[key] = false;
          // ukloni blogove tog autora iz filtered (osim mojih)
          this.filtered = this.filtered.filter(b =>
            !(String(b.authorId) === key && String(b.authorId) !== uid)
          );
        }
      });
    } else {
      this.followersService.follow(uid, key).subscribe({
        next: () => {
          this.following[key] = true;
          // dodaj blogove tog autora iz punog skupa
          const add = this.blogs.filter(b => String(b.authorId) === key);
          this.filtered = [...add, ...this.filtered].sort((a: any, b: any) =>
            new Date(b?.createdAt ?? 0).getTime() - new Date(a?.createdAt ?? 0).getTime()
          );
        }
      });
    }
  }

  isFollowing(authorId: any): boolean {
    return !!this.following[String(authorId)];
  }

  canDelete(blog: any): boolean {
    return this.currentUserId != null && String(blog?.authorId) === this.currentUserId;
  }

  // ako želiš i komentare, koristi istu logiku kao na listi:
  canComment(authorId: string | number): boolean {
    if (this.currentUserId == null) return false;
    if (String(this.currentUserId) === String(authorId)) return true;
    return this.isFollowing(authorId);
  }
}
