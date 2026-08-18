import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../dashboard.service';
import { DashboardSummary } from '../dashboard.model';
import { WidgetHostComponent } from '../widget-host/widget-host';
import { WidgetConfig } from '../widgets/widget.model';
import { StatCardWidgetComponent } from '../widgets/stat-card-widget/stat-card-widget';
import { LowStockWidgetComponent } from '../widgets/low-stock-widget/low-stock-widget';
import { RevenueWidgetComponent } from '../widgets/revenue-widget/revenue-widget';

const formatInr = (n: number) => `₹${Math.round(n).toLocaleString('en-IN')}`;

@Component({
  selector: 'app-dashboard-page',
  imports: [CommonModule, FormsModule, RouterLink, WidgetHostComponent],
  templateUrl: './dashboard-page.html',
})
export class DashboardPageComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  loading = signal(true);
  errorMessage = signal('');
  summary = signal<DashboardSummary | null>(null);
  dateFrom = signal('');
  dateTo = signal('');

  // The config array WidgetHostComponent turns into real components - swap,
  // reorder, or add a widget here without touching WidgetHostComponent itself.
  kpiWidgets = computed<WidgetConfig[]>(() => {
    const s = this.summary();
    if (!s) return [];
    return [
      {
        component: StatCardWidgetComponent,
        inputs: { label: 'Total Revenue', value: formatInr(s.totalRevenue), hint: `${s.totalOrders} order(s)`, icon: 'currency-rupee', iconColorClass: 'text-success', colorClass: 'revenue' },
      },
      {
        component: StatCardWidgetComponent,
        inputs: { label: 'Total Orders', value: String(s.totalOrders), hint: `${s.pendingOrders} pending`, icon: 'bag-check', iconColorClass: 'text-primary', colorClass: 'orders' },
      },
      {
        component: StatCardWidgetComponent,
        inputs: { label: 'Customers', value: String(s.totalCustomers), icon: 'people', iconColorClass: 'text-info', colorClass: 'customers' },
      },
      {
        component: StatCardWidgetComponent,
        inputs: { label: 'Product Inventory', value: String(s.totalProducts), hint: `${s.outOfStockProducts} out of stock`, icon: 'box-seam', iconColorClass: 'text-warning', colorClass: 'products' },
      },
    ];
  });

  detailWidgets = computed<WidgetConfig[]>(() => {
    const s = this.summary();
    if (!s) return [];
    return [
      { component: RevenueWidgetComponent, inputs: { points: s.monthlyRevenue } },
      { component: LowStockWidgetComponent, inputs: { products: s.lowStockProducts } },
    ];
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    this.dashboardService.getSummary(this.dateFrom() || undefined, this.dateTo() || undefined).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.summary.set(res.data);
        else this.errorMessage.set(res.error?.message ?? 'Failed to load dashboard.');
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load dashboard. Is the API running?');
      },
    });
  }

  applyRange(): void {
    this.load();
  }

  resetRange(): void {
    this.dateFrom.set('');
    this.dateTo.set('');
    this.load();
  }
}
