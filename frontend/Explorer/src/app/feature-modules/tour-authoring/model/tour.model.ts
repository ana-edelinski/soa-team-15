import { KeyPoint } from "./keypoint.model";

export interface Tour {
    id?: number;
    name: string;
    description: string;
    difficulty: string;
    tags: number[]; 
    status: number; 
    price: number;
    discountedPrice?: number;
    originalPrice?: number; 
    userId: number;
    // lengthInKm: number;
    publishedTime?: Date;
    archiveTime?: Date;
    // equipmentIds: number[] | null; 
    // keyPoints: KeyPoint[];
    selected?: boolean;
  }