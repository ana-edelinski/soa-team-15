export interface KeyPoint {
    id?: number;
    name: string;
    longitude: number;
    latitude: number;
    description: string;
    image: string;
    userId: number;
    tourId: number;
    imageBase64?: string;
    publicStatus: PublicStatus;
}
export enum PublicStatus {
    PRIVATE = 0,
    REQUESTED_PUBLIC = 1,
    PUBLIC = 2
}