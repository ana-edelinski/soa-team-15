import { Tour } from "../../tour-authoring/model/tour.model";

export interface OrderItem{
    id?: number;
    tourName: string;
    price: number;
    tourId: number;
    cartId: number;
    authorId?: number;
    tourDetails?: Tour;
}