import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stat-card-widget',
  imports: [CommonModule],
  template: `
    <div class="col-sm-6 col-xl-3">
      <div class="kpi-card" [class]="colorClass">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <div class="kpi-label">{{ label }}</div>
            <div class="kpi-value">{{ value }}</div>
            <small class="text-muted" *ngIf="hint">{{ hint }}</small>
          </div>
          <div class="kpi-icon" [class]="iconColorClass"><i class="bi" [class]="'bi-' + icon"></i></div>
        </div>
      </div>
    </div>
  `,
})
export class StatCardWidgetComponent {
  @Input() label = '';
  @Input() value = '';
  @Input() hint = '';
  @Input() icon = 'graph-up';
  @Input() iconColorClass = 'text-primary';
  /** One of the ShopEase kpi-card variants (revenue/orders/customers/products) - controls the left accent border. */
  @Input() colorClass = 'revenue';
}
