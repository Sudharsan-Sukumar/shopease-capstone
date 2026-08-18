import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ReportsService } from '../reports.service';
import { InventoryReport, SalesSummary } from '../reports.model';

@Component({
  selector: 'app-reports-page',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './reports-page.html',
})
export class ReportsPageComponent implements OnInit {
  private reportsService = inject(ReportsService);

  activeTab = signal<'sales' | 'inventory'>('sales');
  errorMessage = signal('');

  dateFrom = signal('');
  dateTo = signal('');
  loadingSales = signal(false);
  salesReport = signal<SalesSummary | null>(null);
  exporting = signal(false);

  loadingInventory = signal(false);
  inventoryReport = signal<InventoryReport | null>(null);

  ngOnInit(): void {
    this.runSalesReport();
  }

  selectTab(tab: 'sales' | 'inventory'): void {
    this.activeTab.set(tab);
    if (tab === 'inventory' && !this.inventoryReport()) this.loadInventoryReport();
  }

  runSalesReport(): void {
    this.loadingSales.set(true);
    this.errorMessage.set('');
    this.reportsService.getSalesReport(this.dateFrom() || undefined, this.dateTo() || undefined).subscribe({
      next: (res) => {
        this.loadingSales.set(false);
        if (res.success && res.data) this.salesReport.set(res.data);
        else this.errorMessage.set(res.error?.message ?? 'Failed to load sales report.');
      },
      error: () => {
        this.loadingSales.set(false);
        this.errorMessage.set('Failed to load sales report. Is the API running?');
      },
    });
  }

  clearRange(): void {
    this.dateFrom.set('');
    this.dateTo.set('');
    this.runSalesReport();
  }

  private loadInventoryReport(): void {
    this.loadingInventory.set(true);
    this.reportsService.getInventoryReport().subscribe({
      next: (res) => {
        this.loadingInventory.set(false);
        if (res.success && res.data) this.inventoryReport.set(res.data);
        else this.errorMessage.set(res.error?.message ?? 'Failed to load inventory report.');
      },
      error: () => {
        this.loadingInventory.set(false);
        this.errorMessage.set('Failed to load inventory report. Is the API running?');
      },
    });
  }

  exportCsv(): void {
    this.exporting.set(true);
    this.reportsService.exportSalesCsv(this.dateFrom() || undefined, this.dateTo() || undefined).subscribe({
      next: (blob) => {
        this.exporting.set(false);
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `sales-report-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.exporting.set(false);
        this.errorMessage.set('Could not export CSV.');
      },
    });
  }
}
