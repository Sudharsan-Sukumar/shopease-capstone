import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../auth/auth.store';
import { Category, Product } from './product.model';

/** The only thing in this feature that touches HttpClient - components call this, never HttpClient directly. */
@Injectable({ providedIn: 'root' })
export class ProductService {
  private http = inject(HttpClient);

  /** `search` is server-side (see ProductsController) - the caller (ProductListComponent) is what debounces/switchMaps it. */
  getAll(search?: string) {
    const params = search ? new HttpParams().set('search', search) : undefined;
    return this.http.get<ApiResponse<Product[]>>(`${environment.apiBaseUrl}/products`, { params });
  }

  getById(id: number) {
    return this.http.get<ApiResponse<Product>>(`${environment.apiBaseUrl}/products/${id}`);
  }

  getCategories() {
    return this.http.get<ApiResponse<Category[]>>(`${environment.apiBaseUrl}/categories`);
  }
}
