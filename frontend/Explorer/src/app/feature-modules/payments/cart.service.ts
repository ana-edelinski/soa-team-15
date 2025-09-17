import { Injectable } from '@angular/core';
import { env } from '../../../environments/environment';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';

import { Tour } from '../../core/models/tour.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartItems: any[] = []; 
  private tours: Tour[] = [];

  //pracenje broja artikala i preview 
  private cartItemCount = new BehaviorSubject<number>(0); 
  cartItemCount$ = this.cartItemCount.asObservable(); 
  //private cartItemsSubject = new BehaviorSubject<OrderItem[]>([]);
  //cartItems$ = this.cartItemsSubject.asObservable();

  constructor(private http: HttpClient) {}

  setTours(tours: Tour[]): void {
    this.tours = tours;
  }

  // createShoppingCart(cart: ShoppingCart): Observable<ShoppingCart>{
  //   return this.http.post<ShoppingCart>(environment.apiHost + 'shopping', cart);
  // }

  // addToCart(item: OrderItem): Observable<OrderItem> {
  //   //this.cartItems.push(item);
  //   return this.http.post<OrderItem>(environment.apiHost + 'shopping/item', item)
  //   .pipe(
  //     tap(() => {
  //       this.updateCartItemCount(1); // Povećavamo broj artikala
  //       this.getCartItems(item.cartId).subscribe(); // Automatski osveži korpu
  //     })
  //   );
  // }

  // removeFromCart(itemId: number): Observable<void> {
  //   return this.http.delete<void>(environment.apiHost + 'shopping/item/' + itemId)
  //   .pipe(
  //     tap(() => {
  //       this.updateCartItemCount(-1); // Smanjujemo broj artikala
  //     })
  //   );
  // }

  
  // getCartItems(cartId: number): Observable<OrderItem[]> {
  //   //return this.cartItems;
  //   return this.http.get<OrderItem[]>(environment.apiHost + 'shopping/item/getAllFromCart/' + cartId)
  //   .pipe(
  //     tap((items) => {
  //       this.cartItemCount.next(items.length); // Postavljamo inicijalni broj artikala
  //       this.updateCartItems(items);
  //     })
  //   );

  //   getCartsByUser(userId: number): Observable<ShoppingCart[]>
  // {
  //   return this.http.get<ShoppingCart[]>(environment.apiHost + 'shopping/getByUser/' + userId);
  // }
}