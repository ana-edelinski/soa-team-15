export interface TourExecution {
  id?: number;
  tourId: number;
  touristId?: number;
  locationId?: number;
  lastActivity?: Date;
  status?: number;            
//   completedKeys?: CompletedKeys[];
}

// export interface CompletedKeys {
//   keyPointId: number;
//   completionTime: Date;
// }
