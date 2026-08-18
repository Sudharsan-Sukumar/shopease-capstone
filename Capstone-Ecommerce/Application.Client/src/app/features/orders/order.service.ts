import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../auth/auth.store';
import { Order } from './order.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);

  checkout() {
    return this.http.post<ApiResponse<Order>>(`${environment.apiBaseUrl}/orders/checkout`, {});
  }

  getMyOrders() {
    return this.http.get<ApiResponse<Order[]>>(`${environment.apiBaseUrl}/orders`);
  }
}
