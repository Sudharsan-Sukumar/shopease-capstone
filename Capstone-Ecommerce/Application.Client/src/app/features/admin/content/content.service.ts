import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../auth/auth.store';
import { ContentBlock } from './content.model';

@Injectable({ providedIn: 'root' })
export class ContentService {
  private http = inject(HttpClient);

  /** Public - only active blocks, in display order. */
  getActive() {
    return this.http.get<ApiResponse<ContentBlock[]>>(`${environment.apiBaseUrl}/content`);
  }

  /** Staff - everything, including inactive/draft blocks. */
  getAll() {
    return this.http.get<ApiResponse<ContentBlock[]>>(`${environment.apiBaseUrl}/content/all`);
  }

  create(payload: { key: string; title: string; body: string; imageUrl: string | null; isActive: boolean; displayOrder: number }) {
    return this.http.post<ApiResponse<ContentBlock>>(`${environment.apiBaseUrl}/content`, payload);
  }

  update(id: number, payload: { title: string; body: string; imageUrl: string | null; isActive: boolean; displayOrder: number }) {
    return this.http.put<ApiResponse<ContentBlock>>(`${environment.apiBaseUrl}/content/${id}`, payload);
  }

  delete(id: number) {
    return this.http.delete<ApiResponse<null>>(`${environment.apiBaseUrl}/content/${id}`);
  }
}
