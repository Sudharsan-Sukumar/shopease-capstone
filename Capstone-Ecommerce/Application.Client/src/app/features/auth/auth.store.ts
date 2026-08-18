import { computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { environment } from '../../../environments/environment';

export interface UserInfo {
  id: number;
  email: string;
  fullName: string;
  phone: string;
  roles: string[];
}

export interface AuthResult {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  user: UserInfo;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  error: { code: string; message: string } | null;
}

const ACCESS_TOKEN_KEY = 'shopease_access_token';
const REFRESH_TOKEN_KEY = 'shopease_refresh_token';
const USER_KEY = 'shopease_user';

function readStoredUser(): UserInfo | null {
  const raw = localStorage.getItem(USER_KEY);
  return raw ? JSON.parse(raw) : null;
}

interface AuthState {
  user: UserInfo | null;
}

/**
 * NgRx SignalStore instead of a hand-rolled Injectable+signal() service
 * (which is what this was in Phase 1/2 - see memory/PR history). Same
 * external shape as before (authStore.user()/isLoggedIn()/isAdmin(), same
 * methods) so every consumer only needed an import swap, not a rewrite.
 * `withState` gives `user` as a signal automatically; `withComputed` adds
 * derived signals; `withMethods` is where HttpClient/Router get injected
 * and where `patchState` performs the actual state mutations - state is
 * never written to directly from outside the store.
 */
export const AuthStore = signalStore(
  { providedIn: 'root' },
  // A factory, not a plain object - withState's argument is otherwise
  // evaluated exactly once at module-load time, so a plain object here would
  // freeze `user` to whatever localStorage held on first import instead of
  // re-reading it every time a new store instance is created (only harmless
  // in the real app because a fresh page load always re-evaluates the
  // module anyway - it broke a Karma test injecting the store mid-run,
  // which is exactly the scenario a factory guards against).
  withState<AuthState>(() => ({ user: readStoredUser() })),
  withComputed(({ user }) => ({
    isLoggedIn: computed(() => user() !== null),
    isAdmin: computed(() => user()?.roles.includes('Admin') ?? false),
    // Any staff tier - can at least see the admin Users page (view + revoke).
    isStaff: computed(() => {
      const roles = user()?.roles ?? [];
      return ['Admin', 'SubAdmin', 'Supervisor', 'SupportAgent'].some((r) => roles.includes(r));
    }),
    // Drives which nav block (shopping vs admin) shows - a staff-only account
    // (the common case) never holds Customer, so it never sees Catalog/Cart/My
    // Orders at all, matching the ShopEase prototype's separate admin console.
    isCustomer: computed(() => user()?.roles.includes('Customer') ?? false),
    // Only these two can create staff accounts or edit anyone's roles - mirrors RoleNames.UserManagers server-side.
    canManageUsers: computed(() => {
      const roles = user()?.roles ?? [];
      return roles.includes('Admin') || roles.includes('SubAdmin');
    }),
    // Same role set as canManageUsers today, but named for its own concern -
    // mirrors RoleNames.ProductManagers server-side. Supervisor/Support Agent
    // get zero product access, not even view, unlike Users/Content.
    canManageProducts: computed(() => {
      const roles = user()?.roles ?? [];
      return roles.includes('Admin') || roles.includes('SubAdmin');
    }),
  })),
  withMethods((store) => {
    const http = inject(HttpClient);
    const router = inject(Router);

    const clearSession = (): void => {
      localStorage.removeItem(ACCESS_TOKEN_KEY);
      localStorage.removeItem(REFRESH_TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
      patchState(store, { user: null });
    };

    return {
      register(payload: { fullName: string; email: string; phone: string; password: string }) {
        return http.post<ApiResponse<AuthResult>>(`${environment.apiBaseUrl}/auth/register`, payload);
      },

      login(payload: { email: string; password: string }) {
        return http.post<ApiResponse<AuthResult>>(`${environment.apiBaseUrl}/auth/login`, payload);
      },

      refresh() {
        return http.post<ApiResponse<AuthResult>>(`${environment.apiBaseUrl}/auth/refresh`, {
          refreshToken: localStorage.getItem(REFRESH_TOKEN_KEY),
        });
      },

      logout(): void {
        const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
        if (refreshToken) {
          http.post(`${environment.apiBaseUrl}/auth/logout`, { refreshToken }).subscribe();
        }
        clearSession();
        router.navigate(['/login']);
      },

      storeSession(result: AuthResult): void {
        localStorage.setItem(ACCESS_TOKEN_KEY, result.accessToken);
        localStorage.setItem(REFRESH_TOKEN_KEY, result.refreshToken);
        localStorage.setItem(USER_KEY, JSON.stringify(result.user));
        patchState(store, { user: result.user });
      },

      clearSession,

      getAccessToken(): string | null {
        return localStorage.getItem(ACCESS_TOKEN_KEY);
      },

      getRefreshToken(): string | null {
        return localStorage.getItem(REFRESH_TOKEN_KEY);
      },
    };
  }),
);
