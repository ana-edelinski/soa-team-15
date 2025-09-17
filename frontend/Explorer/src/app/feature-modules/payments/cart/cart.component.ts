import { Component } from '@angular/core';

@Component({
  selector: 'xp-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent {
  cartItems = [
    { tourName: 'Tura po Fruškoj gori', price: 50 },
    { tourName: 'Tura kroz Novi Sad', price: 30 }
  ];

  get totalPrice(): number {
    return this.cartItems.reduce((sum, item) => sum + item.price, 0);
  }

  removeCartItem(item: any) {
    this.cartItems = this.cartItems.filter(i => i !== item);
  }

  checkout() {
    alert('Checkout clicked!');
  }
}
