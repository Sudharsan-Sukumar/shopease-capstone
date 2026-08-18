import { computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../auth/auth.store';
import { Cart } from './cart.model';

interface CartState {
  cart: Cart;
}

const initialState: CartState = { cart: { items: [], total: 0 } };

/**
 * NgRx SignalStore, same shape/rationale as AuthStore - every mutation
 * still round-trips to the API first, then patches state from the
 * SERVER's response via `setCart` (no optimistic-update guessing).
 * `cart` stays a single nested signal (not flattened to `items`/`total`
 * separately) so every template that already does `cart().items` /
 * `cart().total` kept working unchanged after the refactor.
 */
export const CartStore = signalStore(
  { providedIn: 'root' },
  withState<CartState>(initialState),
  withComputed(({ cart }) => ({
    itemCount: computed(() => cart().items.reduce((sum, i) => sum + i.quantity, 0)),
  })),
  withMethods((store) => {
    const http = inject(HttpClient);

    return {
      refresh(): void {
        http.get<ApiResponse<Cart>>(`${environment.apiBaseUrl}/cart`).subscribe({
          next: (res) => {
            if (res.success && res.data) patchState(store, { cart: res.data });
          },
        });
      },

      addItem(productId: number, quantity: number) {
        return http.post<ApiResponse<Cart>>(`${environment.apiBaseUrl}/cart/items`, { productId, quantity });
      },

      updateItem(productId: number, quantity: number) {
        return http.put<ApiResponse<Cart>>(`${environment.apiBaseUrl}/cart/items/${productId}`, { quantity });
      },

      removeItem(productId: number) {
        return http.delete<ApiResponse<Cart>>(`${environment.apiBaseUrl}/cart/items/${productId}`);
      },

      clear() {
        return http.delete<ApiResponse<object>>(`${environment.apiBaseUrl}/cart`);
      },

      setCart(cart: Cart): void {
        patchState(store, { cart });
      },
    };
  }),
);
