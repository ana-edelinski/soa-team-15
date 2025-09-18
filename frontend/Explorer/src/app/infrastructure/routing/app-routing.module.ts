import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from 'src/app/feature-modules/layout/home/home.component';
import { LoginComponent } from '../auth/login/login.component';
import { EquipmentComponent } from 'src/app/feature-modules/administration/equipment/equipment.component';
import { AuthGuard } from '../auth/auth.guard';
import { RegistrationComponent } from '../auth/registration/registration.component';
import { CreateComponent } from 'src/app/blog/create/create.component';
import { ListComponent } from 'src/app/blog/list/list.component';
import { UserListComponent } from 'src/app/feature-modules/administration/user-list/user-list.component';
import { ProfileInfoComponent } from 'src/app/feature-modules/layout/profile-info/profile-info.component';
import { CreateTourComponent } from 'src/app/feature-modules/tour-authoring/create-tour/create-tour.component';
import { MyToursComponent } from 'src/app/feature-modules/tour-authoring/my-tours/my-tours.component';
import { PositionSimulatorComponent } from 'src/app/feature-modules/tour-execution/position-simulator/position-simulator.component';
import { BrowseToursComponent } from 'src/app/feature-modules/tour-browse/browse-tours/browse-tours.component';
import { CreateKeyPointsComponent } from 'src/app/feature-modules/tour-authoring/create-key-points/create-key-points.component';
import { ViewKeyPointsComponent } from 'src/app/feature-modules/tour-authoring/view-key-points/view-key-points.component';
import { PurchasedToursComponent } from 'src/app/feature-modules/tour-execution/purchased-tours/purchased-tours.component';

const routes: Routes = [
  { path: '', redirectTo: '/blogs', pathMatch: 'full' },   
  { path: 'blogs', component: ListComponent },             
  { path: 'blogs/create', component: CreateComponent },    
  {path: 'home', component: HomeComponent},
  {path: 'login', component: LoginComponent},
  {path: 'register', component: RegistrationComponent},
  {path: 'equipment', component: EquipmentComponent, canActivate: [AuthGuard],},
  { path: 'admin/users', component: UserListComponent },
  { path: 'profile', component: ProfileInfoComponent },
  { path: 'create-tour', component: CreateTourComponent },
  { path: 'my-tours', component: MyToursComponent },
  { path: 'purchased-tours', component: PurchasedToursComponent },
  { path: 'position-simulator', component: PositionSimulatorComponent},
  { path: 'tours', component: BrowseToursComponent },
  { path: 'tours/:id/key-points', component: CreateKeyPointsComponent },
  { path: 'tours/:id/key-points/view', component: ViewKeyPointsComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
