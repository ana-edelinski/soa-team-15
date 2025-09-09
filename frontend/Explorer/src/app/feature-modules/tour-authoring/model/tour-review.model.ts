export interface TourReviewCreate {
  idTourist: number;
  rating: number;
  comment?: string;
  dateTour: string;        // ISO
  images: string[];        // 👈 više slika
}

export interface TourReview {
  id: number;
  idTour: number;
  idTourist: number;
  rating: number;
  comment?: string;
  dateTour?: string;
  dateComment?: string;
  images: string[];        // 👈
}
