import { Component } from '@angular/core';
import { Router, NavigationEnd, Event as RouterEvent } from '@angular/router';
import { Location } from '@angular/common';
import { map, filter, startWith } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';

@Component({
  selector: 'xp-position-fab',
  templateUrl: './position-fab.component.html',
  styleUrls: ['./position-fab.component.css']
})
export class PositionFabComponent {
  show$: Observable<boolean> = this.auth.user$.pipe(
    map(u => !!u && u.role?.toLowerCase() === 'tourist')
  );

  isOnSimulator = false;
  private lastNonSimulatorUrl: string | null = null;

  constructor(
    private auth: AuthService,
    private router: Router,
    private location: Location
  ) {
    this.router.events
      .pipe(
        filter((e: RouterEvent): e is NavigationEnd => e instanceof NavigationEnd),
        startWith(null)
      )
      .subscribe(() => {
        const url = this.router.url;
        const onSim = url.startsWith('/position-simulator');
        this.isOnSimulator = onSim;

        // Pamti poslednju rutu koja NIJE simulator (za povratak)
        if (!onSim) this.lastNonSimulatorUrl = url;
      });
  }

  toggleSimulator() {
    if (this.isOnSimulator) {
      if (this.lastNonSimulatorUrl) {
        this.router.navigateByUrl(this.lastNonSimulatorUrl);
      } else {
        // pokušaj istorijski back; ako nema istorije, fallback na root
        if (!this.location.getState()) {
          this.router.navigateByUrl('/');
        } else {
          this.location.back();
        }
      }
    } else {
      this.router.navigateByUrl('/position-simulator');
    }
  }
}
