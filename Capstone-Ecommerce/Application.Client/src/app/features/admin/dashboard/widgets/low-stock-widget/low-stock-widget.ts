import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LowStockProduct } from '../../dashboard.model';

@Component({
  selector: 'app-low-stock-widget',
  imports: [CommonModule],
  template: `
    <div class="col-lg-6">
      <div class="card h-100">
        <div class="card-header bg-white"><h5 class="mb-0"><i class="bi bi-exclamation-triangle text-danger"></i> Stock Alerts</h5></div>
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-hover mb-0">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Stock</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let p of products">
                  <td>{{ p.name }}</td>
                  <td class="fw-bold" [class.text-danger]="p.stock === 0" [class.text-warning]="p.stock > 0">{{ p.stock }}</td>
                  <td>
                    <span class="badge" [class.bg-danger]="p.stock === 0" [class.bg-warning]="p.stock > 0" [class.text-dark]="p.stock > 0">
                      {{ p.stock === 0 ? 'Out of Stock' : 'Low Stock' }}
                    </span>
                  </td>
                </tr>
                <tr *ngIf="products.length === 0">
                  <td colspan="3" class="text-center text-muted py-4"><i class="bi bi-check-circle text-success"></i> All products are well stocked!</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class LowStockWidgetComponent {
  @Input() products: LowStockProduct[] = [];
}
