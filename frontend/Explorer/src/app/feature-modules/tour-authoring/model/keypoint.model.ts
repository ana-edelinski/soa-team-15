export interface KeyPoint {
    id?: number;
    name: string;
    longitude: number;
    latitude: number;
    description: string;
    userId?: number | null;
    tourId?: number | null;
    pictureFile?: File | null;
}


export enum PublicStatus {
    PRIVATE = 0,
    REQUESTED_PUBLIC = 1,
    PUBLIC = 2
}