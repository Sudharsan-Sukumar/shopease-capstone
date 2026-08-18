import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CartStore } from '../cart.store';
import { CartItem } from '../cart.model';

@Component({
  selector: 'app-cart-page',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './cart-page.html',
})
export class CartPageComponent implements OnInit {
  private cartService = inject(CartStore);

  cart = this.cartService.cart;

  ngOnInit(): void {
    this.cartService.refresh();
  }

  changeQuantity(item: CartItem, delta: number): void {
    const next = item.quantity + delta;
    if (next < 1 || next > item.availableStock) return;

    this.cartService.updateItem(item.productId, next).subscribe({
      next: (res) => {
        if (res.success && res.data) this.cartService.setCart(res.data);
      },
    });
  }

  removeItem(item: CartItem): void {
    this.cartService.removeItem(item.productId).subscribe({
      next: (res) => {
        if (res.success && res.data) this.cartService.setCart(res.data);
      },
    });
  }

  clearCart(): void {
    this.cartService.clear().subscribe({
      next: () => this.cartService.refresh(),
    });
  }
}
