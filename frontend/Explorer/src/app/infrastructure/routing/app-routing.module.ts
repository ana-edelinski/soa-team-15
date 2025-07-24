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

const routes: Routes = [
  { path: '', redirectTo: '/blogs', pathMatch: 'full' },   
  { path: 'blogs', component: ListComponent },             
  { path: 'blogs/create', component: CreateComponent },    
  {path: 'home', component: HomeComponent},
  {path: 'login', component: LoginComponent},
  {path: 'register', component: RegistrationComponent},
  {path: 'equipment', component: EquipmentComponent, canActivate: [AuthGuard],},
  { path: 'admin/users', component: UserListComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
