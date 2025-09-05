export interface TourReviewCreate {
  idTourist: number;
  rating: number;          // 1..5
  comment?: string;
  dateTour: string;        // ISO "2025-08-20T00:00:00Z"
  image?: string;          // URL 
}

export interface TourReview {
  id: number;
  idTour: number;
  idTourist: number;
  rating: number;
  comment?: string;
  dateTour?: string;
  dateComment?: string;
  image?: string;
}
