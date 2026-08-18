import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthStore } from './features/auth/auth.store';
import { CartStore } from './features/cart/cart.store';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected auth = inject(AuthStore);
  protected cart = inject(CartStore);

  constructor() {
    // Covers a fresh page load with an existing (localStorage-backed)
    // session - login/register trigger their own refresh right after
    // storeSession, so the badge is correct there too.
    if (this.auth.isLoggedIn()) this.cart.refresh();
  }

  logout(): void {
    this.auth.logout();
    this.cart.setCart({ items: [], total: 0 });
  }
}
