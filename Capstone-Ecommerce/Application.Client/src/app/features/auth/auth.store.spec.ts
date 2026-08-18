import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthResult, AuthStore } from './auth.store';
import { environment } from '../../../environments/environment';

describe('AuthStore', () => {
  let httpMock: HttpTestingController;
  let routerSpy: jasmine.SpyObj<Router>;

  const sampleResult: AuthResult = {
    accessToken: 'access-token',
    refreshToken: 'refresh-token',
    accessTokenExpiresAtUtc: new Date().toISOString(),
    user: { id: 1, email: 'admin@shopease.com', fullName: 'Admin', phone: '', roles: ['Admin'] },
  };

  beforeEach(() => {
    localStorage.clear();
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: Router, useValue: routerSpy }],
    });

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('starts logged out when localStorage has no stored user', () => {
    const store = TestBed.inject(AuthStore);

    expect(store.isLoggedIn()).toBeFalse();
    expect(store.user()).toBeNull();
  });

  it('restores the session from localStorage on creation', () => {
    localStorage.setItem('shopease_user', JSON.stringify(sampleResult.user));

    const store = TestBed.inject(AuthStore);

    expect(store.isLoggedIn()).toBeTrue();
    expect(store.user()?.email).toBe('admin@shopease.com');
  });

  it('storeSession persists tokens and user, and marks the store logged in', () => {
    const store = TestBed.inject(AuthStore);

    store.storeSession(sampleResult);

    expect(store.isLoggedIn()).toBeTrue();
    expect(store.getAccessToken()).toBe('access-token');
    expect(store.getRefreshToken()).toBe('refresh-token');
    expect(localStorage.getItem('shopease_user')).toContain('admin@shopease.com');
  });

  it('isAdmin is true only for a user holding the Admin role', () => {
    const store = TestBed.inject(AuthStore);

    store.storeSession(sampleResult); // roles: ['Admin']
    expect(store.isAdmin()).toBeTrue();

    store.storeSession({ ...sampleResult, user: { ...sampleResult.user, roles: ['Customer'] } });
    expect(store.isAdmin()).toBeFalse();
  });

  it('isStaff is true for any staff-tier role and false for Customer only', () => {
    const store = TestBed.inject(AuthStore);

    store.storeSession({ ...sampleResult, user: { ...sampleResult.user, roles: ['SupportAgent'] } });
    expect(store.isStaff()).toBeTrue();

    store.storeSession({ ...sampleResult, user: { ...sampleResult.user, roles: ['Customer'] } });
    expect(store.isStaff()).toBeFalse();
  });

  it('canManageUsers is true only for Admin/Sub Admin, not Supervisor/Support Agent', () => {
    const store = TestBed.inject(AuthStore);

    store.storeSession({ ...sampleResult, user: { ...sampleResult.user, roles: ['SubAdmin'] } });
    expect(store.canManageUsers()).toBeTrue();

    store.storeSession({ ...sampleResult, user: { ...sampleResult.user, roles: ['Supervisor'] } });
    expect(store.canManageUsers()).toBeFalse();
  });

  it('clearSession removes tokens and logs the store out', () => {
    const store = TestBed.inject(AuthStore);
    store.storeSession(sampleResult);

    store.clearSession();

    expect(store.isLoggedIn()).toBeFalse();
    expect(store.getAccessToken()).toBeNull();
  });

  it('logout posts the refresh token, clears the session, and navigates to /login', () => {
    const store = TestBed.inject(AuthStore);
    store.storeSession(sampleResult);

    store.logout();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/logout`);
    expect(req.request.body.refreshToken).toBe('refresh-token');
    req.flush({});

    expect(store.isLoggedIn()).toBeFalse();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('login posts credentials to the API', () => {
    const store = TestBed.inject(AuthStore);

    store.login({ email: 'admin@shopease.com', password: 'Admin@123' }).subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'admin@shopease.com', password: 'Admin@123' });
    req.flush({ success: true, data: sampleResult, error: null });
  });
});
