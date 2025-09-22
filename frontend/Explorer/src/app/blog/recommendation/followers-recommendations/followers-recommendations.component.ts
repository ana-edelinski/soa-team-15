import { Component, OnInit } from '@angular/core';
import { LayoutService } from 'src/app/feature-modules/layout/layout.service';
import { TokenStorage } from 'src/app/infrastructure/auth/jwt/token.service';
import { FollowersService } from '../../followers.service';

type RecommendedUser = {
  id: string;
  userName?: string;
  // Po želji možeš dodati avatarUrl itd.
};

@Component({
  selector: 'app-followers-recommendations',
  templateUrl: './followers-recommendations.component.html',
  styleUrls: ['./followers-recommendations.component.css']
})
export class FollowersRecommendationsComponent implements OnInit {
  loading = false;
  currentUserId: string | null = null;

  // lista preporučenih naloga
  recs: RecommendedUser[] = [];

  // mapa: userId -> da li ga već pratim
  following: Record<string, boolean> = {};

  constructor(
    private followersService: FollowersService,
    private profileService: LayoutService,
    private tokenStorage: TokenStorage
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.extractUserId();
    this.loadRecommendations();
  }

  private extractUserId(): string | null {
    const token = this.tokenStorage.getAccessToken?.();
    if (!token) return null;
    try {
      const payloadBase64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = JSON.parse(atob(payloadBase64));
      for (const k of ['id', 'userId', 'nameid', 'sub']) {
        if (json?.[k] !== undefined && json?.[k] !== null) return String(json[k]);
      }
    } catch {}
    return null;
  }

  loadRecommendations(): void {
    if (!this.currentUserId) {
      this.recs = [];
      return;
    }
    this.loading = true;

    // 1) povuci preporuke
    this.followersService.getRecommendations(this.currentUserId).subscribe({
      next: (ids) => {
        const uniq = Array.from(new Set((ids ?? []).map(String)));
        this.recs = uniq.map(id => ({ id }));

        uniq.forEach((id) => {
          const numericId = Number(id);                 // 👈 cast na number
          if (Number.isNaN(numericId)) {
            console.warn('Recommendation id is not a number:', id);
            this.following[id] = false;
            return;
          }

          this.profileService.getProfile(numericId).subscribe({
            next: (p: any) => {
              const item = this.recs.find(r => r.id === id);
              if (item) item.userName = p?.userName ?? `Korisnik #${id}`;
            },
            error: () => {
              const item = this.recs.find(r => r.id === id);
              if (item) item.userName = `Korisnik #${id}`;
            }
          });

  this.following[id] = false; // inicijalno
});


        // 2) povuci listu svih koje vec pratim da postavimo ispravne dugmiće
        this.followersService.getFollowingOf(this.currentUserId!).subscribe({
          next: (list) => {
            (list ?? []).forEach(fId => this.following[String(fId)] = true);
            this.loading = false;
          },
          error: () => { this.loading = false; }
        });
      },
      error: () => { this.loading = false; }
    });
  }

  displayName(u: RecommendedUser): string {
    if (u.userName && u.userName.trim().length > 0) return '@' + u.userName;
    return `Korisnik #${u.id}`;
  }

  isFollowing(id: string): boolean {
    return !!this.following[String(id)];
  }

  toggleFollow(targetId: string): void {
    if (!this.currentUserId) return;
    const key = String(targetId);
    if (this.isFollowing(key)) {
      this.followersService.unfollow(this.currentUserId, key).subscribe({
        next: () => { this.following[key] = false; console.log(`UNFOLLOW ${this.currentUserId} -> ${key}`); },
        error: (err) => console.error('Error unfollow:', err)
      });
    } else {
      this.followersService.follow(this.currentUserId, key).subscribe({
        next: () => { this.following[key] = true; console.log(`FOLLOW ${this.currentUserId} -> ${key}`); },
        error: (err) => console.error('Error follow:', err)
      });
    }
  }
}
