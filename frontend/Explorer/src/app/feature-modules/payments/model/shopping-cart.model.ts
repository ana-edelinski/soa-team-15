import { OrderItem } from "./order-item.model";

export interface ShoppingCart {
    id?:number;
    userId?: number;
    items: OrderItem[];
    totalPrice: number;
}