import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface PingResponse {
  status: string;
  timestampUtc: string;
}

/**
 * Phase 0 only: proves Angular can actually reach the API (CORS, ports,
 * routing) before any real feature exists. Lives in shared/ rather than
 * features/ since it isn't tied to one business feature.
 */
@Injectable({ providedIn: 'root' })
export class HealthService {
  private http = inject(HttpClient);

  ping() {
    return this.http.get<PingResponse>(`${environment.apiBaseUrl}/health/ping`);
  }
}
