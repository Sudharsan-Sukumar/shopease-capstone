import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { CartStore } from '../../cart/cart.store';
import { OrderService } from '../../orders/order.service';

@Component({
  selector: 'app-checkout-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './checkout-page.html',
})
export class CheckoutPageComponent implements OnInit {
  private cartService = inject(CartStore);
  private orderService = inject(OrderService);
  private router = inject(Router);

  cart = this.cartService.cart;
  placingOrder = signal(false);
  errorMessage = signal('');

  ngOnInit(): void {
    this.cartService.refresh();
  }

  placeOrder(): void {
    this.placingOrder.set(true);
    this.errorMessage.set('');

    this.orderService.checkout().subscribe({
      next: (res) => {
        this.placingOrder.set(false);
        if (res.success && res.data) {
          this.cartService.setCart({ items: [], total: 0 }); // checkout clears the server-side cart too
          this.router.navigate(['/orders'], { state: { justPlacedOrderId: res.data.id } });
        } else {
          this.errorMessage.set(res.error?.message ?? 'Checkout failed.');
        }
      },
      error: (err) => {
        this.placingOrder.set(false);
        this.errorMessage.set(err.error?.error?.message ?? 'Checkout failed. Please try again.');
      },
    });
  }
}
