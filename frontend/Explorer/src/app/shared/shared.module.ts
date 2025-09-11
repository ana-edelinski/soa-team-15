import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PositionFabComponent } from './position-fab/position-fab.component';
import { MapComponent } from './map/map.component';



@NgModule({
  declarations: [
    PositionFabComponent,
    MapComponent
  ],
  imports: [
    CommonModule
  ]
})
export class SharedModule { }
