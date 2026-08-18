import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../auth/auth.store';
import { Category, Product } from '../../products/product.model';

export interface ProductWritePayload {
  name: string;
  description: string;
  price: number;
  stock: number;
  imageUrl: string | null;
  categoryId: number;
}

@Injectable({ providedIn: 'root' })
export class ProductManagementService {
  private http = inject(HttpClient);

  /** Staff-only - includes deactivated products so they can be found and reactivated. */
  getAll() {
    return this.http.get<ApiResponse<Product[]>>(`${environment.apiBaseUrl}/products/all`);
  }

  getCategories() {
    return this.http.get<ApiResponse<Category[]>>(`${environment.apiBaseUrl}/categories`);
  }

  create(payload: ProductWritePayload) {
    return this.http.post<ApiResponse<Product>>(`${environment.apiBaseUrl}/products`, payload);
  }

  update(id: number, payload: ProductWritePayload & { isActive: boolean }) {
    return this.http.put<ApiResponse<Product>>(`${environment.apiBaseUrl}/products/${id}`, payload);
  }

  /** Soft-delete (sets IsActive false) - order history keeps a real row to point at. */
  deactivate(id: number) {
    return this.http.delete<ApiResponse<null>>(`${environment.apiBaseUrl}/products/${id}`);
  }
}
