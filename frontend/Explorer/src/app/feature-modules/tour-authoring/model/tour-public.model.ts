export interface PublicKeyPoint {
  id: number;
  name: string;
  latitude: number;
  longitude: number;
  description?: string;
  image?: string;
}

export interface PublicTour {
  id: number;
  name: string;
  description?: string;
  difficulty?: string;
  tags?: (number | string)[];
  price?: number;
  lengthInKm?: number;
  publishedAt?: string | null;      // DTO: PublishedAt
  firstKeyPoint?: PublicKeyPoint | null;
}
