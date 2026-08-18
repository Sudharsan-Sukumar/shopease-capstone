import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../auth/auth.store';
import { InventoryReport, SalesSummary } from './reports.model';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private http = inject(HttpClient);

  private dateParams(from?: string, to?: string): HttpParams {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return params;
  }

  getSalesReport(from?: string, to?: string) {
    return this.http.get<ApiResponse<SalesSummary>>(`${environment.apiBaseUrl}/reports/sales`, { params: this.dateParams(from, to) });
  }

  getInventoryReport() {
    return this.http.get<ApiResponse<InventoryReport>>(`${environment.apiBaseUrl}/reports/inventory`);
  }

  /** Returns a raw CSV blob, not an ApiResponse - a file download, not JSON. */
  exportSalesCsv(from?: string, to?: string) {
    return this.http.get(`${environment.apiBaseUrl}/reports/sales/export`, {
      params: this.dateParams(from, to),
      responseType: 'blob',
    });
  }
}
