import { KeyPoint } from "../tour-authoring/model/keypoint.model";

export interface TourExecution {
  id?: number;
  tourId: number;
  touristId?: number;
  locationId?: number;
  lastActivity?: Date;
  status?: number;            
  completedKeys?: CompletedKeys[];
}

export interface CompletedKeys {
  keyPointId: number;
  completionTime: Date;
}

export interface Position {
  id: number;
  latitude: number;
  longitude: number;
  touristId: number;
}

export type TourTag = number | string; // backend može slati enum broj ili string

export interface TourForTourist {
  id: number;
  name: string;
  description?: string | null;
  difficulty?: string | null;
  tags: TourTag[];
  price: number;
  userId: number;
  lengthInKm?: number;        // ako ga šalješ iz backa
  keyPoints: KeyPoint[];
}