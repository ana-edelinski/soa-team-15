import { Injectable } from '@angular/core';
import { environment } from 'src/env/environment';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';

import { Tour } from '../tour-authoring/model/tour.model';
import { OrderItem } from './model/order-item.model';
import { ShoppingCart } from './model/shopping-cart.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly apiBase = 'http://localhost:5027/api/'; 

  private cartItems: any[] = []; 
  private tours: Tour[] = [];

  //pracenje broja artikala i preview 
  private cartItemCount = new BehaviorSubject<number>(0); 
  cartItemCount$ = this.cartItemCount.asObservable(); 
  private cartItemsSubject = new BehaviorSubject<OrderItem[]>([]);
  cartItems$ = this.cartItemsSubject.asObservable();

  constructor(private http: HttpClient) {}

  resetCartState(): void {
    this.cartItemCount.next(0);
    this.cartItemsSubject.next([]);
  }

  setTours(tours: Tour[]): void {
    this.tours = tours;
  }

  createShoppingCart(cart: ShoppingCart): Observable<ShoppingCart>{
    return this.http.post<ShoppingCart>(this.apiBase + 'shopping', cart);
  }

  addToCart(item: OrderItem): Observable<OrderItem> {
    return this.http.post<OrderItem>(this.apiBase + 'shopping/item', item).pipe(
      tap(() => {
        this.updateCartItemCount(1);
        this.getCartItems(item.cartId).subscribe();
      })
    );
  }

  private updateCartItemCount(change: number): void {
    const currentCount = this.cartItemCount.value;
    this.cartItemCount.next(currentCount + change);
  }

  removeFromCart(itemId: number, cartId: number): Observable<void> {
    return this.http.delete<void>(this.apiBase + 'shopping/item/' + itemId).pipe(
      tap(() => {
        // osveži broj artikala
        this.updateCartItemCount(-1);

        // povuci nove stavke iz baze i osveži BehaviorSubject
        this.getCartItems(cartId).subscribe();
      })
    );
  }

  private updateCartItems(items: OrderItem[]): void {
    this.cartItemsSubject.next(items);
  }
  
  getCartItems(cartId: number): Observable<OrderItem[]> {
    //return this.cartItems;
    return this.http.get<OrderItem[]>(this.apiBase + 'shopping/item/getAllFromCart/' + cartId)
    .pipe(
      tap((items) => {
        this.cartItemCount.next(items.length); // Postavljamo inicijalni broj artikala
        this.updateCartItems(items);
      })
    );
  }

  getCartsByUser(userId: number): Observable<ShoppingCart[]>
  {
    return this.http.get<ShoppingCart[]>(this.apiBase + 'shopping/getByUser/' + userId);
  }


}