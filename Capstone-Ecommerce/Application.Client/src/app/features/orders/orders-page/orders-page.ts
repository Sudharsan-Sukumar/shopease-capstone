import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../order.service';
import { Order } from '../order.model';

@Component({
  selector: 'app-orders-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './orders-page.html',
})
export class OrdersPageComponent implements OnInit {
  private orderService = inject(OrderService);

  orders = signal<Order[]>([]);
  loading = signal(true);
  expandedOrderId = signal<number | null>(null);

  ngOnInit(): void {
    this.orderService.getMyOrders().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.orders.set(res.data);
      },
      error: () => this.loading.set(false),
    });
  }

  toggleExpand(orderId: number): void {
    this.expandedOrderId.set(this.expandedOrderId() === orderId ? null : orderId);
  }
}
