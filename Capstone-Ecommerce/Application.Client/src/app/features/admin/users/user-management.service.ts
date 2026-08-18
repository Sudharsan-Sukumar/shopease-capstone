import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../auth/auth.store';
import { AdminUser, Role } from './user.model';

/** The only thing in this feature that touches HttpClient - the component calls this, never HttpClient directly. */
@Injectable({ providedIn: 'root' })
export class UserManagementService {
  private http = inject(HttpClient);

  getAll() {
    return this.http.get<ApiResponse<AdminUser[]>>(`${environment.apiBaseUrl}/users`);
  }

  create(payload: { fullName: string; email: string; phone: string; password: string; roles: string[] }) {
    return this.http.post<ApiResponse<AdminUser>>(`${environment.apiBaseUrl}/users`, payload);
  }

  updateRoles(userId: number, roles: string[]) {
    return this.http.put<ApiResponse<AdminUser>>(`${environment.apiBaseUrl}/users/${userId}/roles`, { roles });
  }

  revokeSessions(userId: number) {
    return this.http.post<ApiResponse<null>>(`${environment.apiBaseUrl}/users/${userId}/revoke-sessions`, {});
  }

  getRoles() {
    return this.http.get<ApiResponse<Role[]>>(`${environment.apiBaseUrl}/roles`);
  }
}
