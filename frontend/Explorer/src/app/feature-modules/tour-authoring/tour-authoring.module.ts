import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Import Material Modules
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule, MatNativeDateModule, MAT_DATE_LOCALE } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatListModule } from '@angular/material/list';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import {MatTabsModule} from '@angular/material/tabs'; 
import { MatDatepickerModule } from '@angular/material/datepicker';


// Import Custom Components
import { CreateTourComponent } from './create-tour/create-tour.component';

// Import Other Modules
import { MaterialModule } from 'src/app/infrastructure/material/material.module';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MyToursComponent } from './my-tours/my-tours.component';
import { ReviewDialogComponent } from './review-dialog/review-dialog.component';
import { CreateKeyPointsComponent } from './create-key-points/create-key-points.component';
import { ViewKeyPointsComponent } from './view-key-points/view-key-points.component';


@NgModule({
  declarations: [
    CreateTourComponent,
    MyToursComponent,
    ReviewDialogComponent
  ],
  imports: [
    MatFormFieldModule,
    CreateKeyPointsComponent,
    ViewKeyPointsComponent,
    FormsModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    CommonModule,
    ReactiveFormsModule,
    MaterialModule,
    RouterModule,
    MatSelectModule,
    MatOptionModule,
    MatCheckboxModule,
    MatListModule,
    MatDialogModule,
    MatExpansionModule,
    MatCardModule,  
    MatDatepickerModule,     // 👈 bitno za [matDatepicker]
    MatNativeDateModule,
    MatSnackBarModule,   
  ],
   providers: [
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' } 
  ]
})
export class TourAuthoringModule { }
