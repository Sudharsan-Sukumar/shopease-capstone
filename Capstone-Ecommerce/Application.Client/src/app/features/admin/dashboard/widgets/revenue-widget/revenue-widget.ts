import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MonthlyRevenuePoint } from '../../dashboard.model';

@Component({
  selector: 'app-revenue-widget',
  imports: [CommonModule],
  template: `
    <div class="col-lg-6">
      <div class="card h-100">
        <div class="card-header bg-white"><h5 class="mb-0"><i class="bi bi-graph-up text-primary"></i> Monthly Revenue</h5></div>
        <div class="card-body">
          <div class="d-flex align-items-end gap-2" *ngIf="points.length > 0; else noData">
            <div class="d-flex flex-column align-items-center flex-fill" *ngFor="let p of points">
              <div class="text-muted small mb-1">&#8377;{{ p.revenue | number: '1.0-0' }}</div>
              <div class="bar-track">
                <div class="bg-primary rounded-top w-100" [style.height.%]="barHeight(p.revenue)"></div>
              </div>
              <div class="text-muted small mt-1">{{ p.month }}</div>
            </div>
          </div>
          <ng-template #noData><p class="text-muted text-center py-5 mb-0">No revenue data yet.</p></ng-template>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .bar-track {
        height: 140px;
        width: 100%;
        display: flex;
        align-items: flex-end;
      }
    `,
  ],
})
export class RevenueWidgetComponent implements OnChanges {
  @Input() points: MonthlyRevenuePoint[] = [];
  private maxRevenue = 1;

  ngOnChanges(): void {
    this.maxRevenue = Math.max(1, ...this.points.map((p) => p.revenue));
  }

  barHeight(revenue: number): number {
    return Math.max(4, (revenue / this.maxRevenue) * 100);
  }
}
