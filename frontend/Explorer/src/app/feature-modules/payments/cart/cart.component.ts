import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CartService } from '../cart.service';
import { OrderItem } from '../model/order-item.model';
import { ShoppingCart } from '../model/shopping-cart.model';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';

type PurchaseToken = { tourId: number; token: string };

@Component({
  selector: 'xp-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent implements OnInit {
  cartItems: OrderItem[] = [];
  shoppingCart: ShoppingCart | null = null;
  user: User | null = null;
  loading = false;

  // prikaz tokena posle uspešnog checkout-a
  purchaseTokens: PurchaseToken[] = [];

  constructor(
    private cartService: CartService,
    private auth: AuthService,
    private snack: MatSnackBar
  ) {
    this.auth.user$.subscribe(u => this.user = u);
  }

  ngOnInit(): void {
    if (!this.user) return; // nije ulogovan

    this.loading = true;
    this.cartService.getCartsByUser(this.user.id).subscribe({
      next: (carts) => {
        if (carts.length > 0) {
          this.shoppingCart = carts[0];
          this.loadCartItems(this.shoppingCart.id!);
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading carts:', err);
        this.loading = false;
      }
    });
  }

  loadCartItems(cartId: number) {
    this.cartService.getCartItems(cartId).subscribe({
      next: (items) => {
        this.cartItems = items;
      },
      error: (err) => {
        console.error('Error loading cart items:', err);
      }
    });
  }

  get totalPrice(): number {
    return this.cartItems.reduce((sum, item) => sum + item.price, 0);
  }

  removeCartItem(item: OrderItem) {
    if (!item.id || !item.cartId) return;

    this.cartService.removeFromCart(item.id, item.cartId).subscribe({
      next: () => {
        this.cartItems = this.cartItems.filter(i => i.id !== item.id);
      },
      error: (err) => {
        console.error('Error removing item:', err);
      }
    });
  }

  checkout() {
    if (!this.user || !this.shoppingCart) {
      this.snack.open('No active cart / not logged in.', 'OK', { duration: 2500 });
      return;
    }

    if (this.cartItems.length === 0) {
      this.snack.open('Cart is empty.', 'OK', { duration: 2000 });
      return;
    }

    this.loading = true;
    this.cartService.checkout(this.shoppingCart.id!, this.user.id).subscribe({
      next: (tokens) => {
        this.purchaseTokens = tokens;   // prikaži korisniku dodeljene tokene
        this.cartItems = [];            // isprazni korpu lokalno
        this.snack.open(`Purchase successful (${tokens.length} token${tokens.length === 1 ? '' : 's'})`, 'OK', { duration: 3000 });
        this.loading = false;
      },
      error: (err) => {
        console.error('Checkout error', err);
        this.snack.open('Checkout failed.', 'OK', { duration: 3000 });
        this.loading = false;
      }
    });
  }
}
