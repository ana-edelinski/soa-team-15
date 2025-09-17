import { Component, OnInit } from '@angular/core';
import { CartService } from '../cart.service';
import { OrderItem } from '../model/order-item.model';
import { ShoppingCart } from '../model/shopping-cart.model';
import { AuthService } from 'src/app/infrastructure/auth/auth.service';
import { User } from 'src/app/infrastructure/auth/model/user.model';

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

  constructor(
    private cartService: CartService,
    private auth: AuthService
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
    alert('Checkout clicked!');
  }
}
