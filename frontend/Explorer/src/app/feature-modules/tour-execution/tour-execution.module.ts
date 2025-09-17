import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PositionSimulatorComponent } from './position-simulator/position-simulator.component';
import { MapComponent } from 'src/app/shared/map/map.component';
import { PurchasedToursComponent } from './purchased-tours/purchased-tours.component';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';



@NgModule({
  declarations: [
    PositionSimulatorComponent,
    MapComponent,
    PurchasedToursComponent
  ],
  imports: [
    CommonModule,      

    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule
  ]
})
export class TourExecutionModule { }
