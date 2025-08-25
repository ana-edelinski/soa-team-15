import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PositionSimulatorComponent } from './position-simulator/position-simulator.component';
import { MapComponent } from 'src/app/shared/map/map.component';



@NgModule({
  declarations: [
    PositionSimulatorComponent,
    MapComponent
  ],
  imports: [
    CommonModule
  ]
})
export class TourExecutionModule { }
