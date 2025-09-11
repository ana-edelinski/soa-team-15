import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CreateComponent } from './blog/create/create.component';

import { AppRoutingModule } from './infrastructure/routing/app-routing.module';
import { AppComponent } from './app.component';
import { LayoutModule } from './feature-modules/layout/layout.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from './infrastructure/material/material.module';
import { AdministrationModule } from './feature-modules/administration/administration.module';
import { BlogModule } from './feature-modules/blog/blog.module';
import { MarketplaceModule } from './feature-modules/marketplace/marketplace.module';
import { TourAuthoringModule } from './feature-modules/tour-authoring/tour-authoring.module';
import { TourExecutionModule } from './feature-modules/tour-execution/tour-execution.module';
import { AuthModule } from './infrastructure/auth/auth.module';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { JwtInterceptor } from './infrastructure/auth/jwt/jwt.interceptor';
import { FormsModule } from '@angular/forms';
import { MarkdownModule } from 'ngx-markdown';
import { ListComponent } from './blog/list/list.component';
import { CommentComponent } from './comment/comment.component';
import { LikeComponent } from './like/like.component';
import { PositionFabComponent } from './shared/position-fab/position-fab.component';
import { BrowseToursComponent } from './feature-modules/tour-browse/browse-tours/browse-tours.component';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

@NgModule({
  declarations: [
    AppComponent,
    CreateComponent,
    ListComponent,
    CommentComponent,
    LikeComponent,
    PositionFabComponent,
    BrowseToursComponent,
  ],
  imports: [
    MarkdownModule.forRoot(),
    BrowserModule,
    FormsModule,
    AppRoutingModule,
    LayoutModule,
    BrowserAnimationsModule,
    MaterialModule,
    AdministrationModule,
    BlogModule,
    MarketplaceModule,
    TourAuthoringModule,
    AuthModule,
    HttpClientModule,
    MatCardModule,
    MatButtonModule,
    MatTooltipModule,
    TourExecutionModule,
    AuthModule,
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true,
    },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
