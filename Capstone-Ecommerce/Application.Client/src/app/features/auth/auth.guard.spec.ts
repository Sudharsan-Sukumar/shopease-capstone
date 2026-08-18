import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { adminGuard, authGuard, staffGuard } from './auth.guard';
import { AuthStore } from './auth.store';

describe('auth guards', () => {
  let routerSpy: jasmine.SpyObj<Router>;
  let authStoreStub: { isLoggedIn: () => boolean; isAdmin: () => boolean; isStaff: () => boolean };

  beforeEach(() => {
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    authStoreStub = { isLoggedIn: () => false, isAdmin: () => false, isStaff: () => false };

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: routerSpy },
        { provide: AuthStore, useValue: authStoreStub },
      ],
    });
  });

  // Guards are plain functions that need an Angular injection context to
  // call inject() - runInInjectionContext supplies that outside a component/route.
  function run<T>(fn: () => T): T {
    return TestBed.runInInjectionContext(fn);
  }

  it('authGuard allows access when logged in', () => {
    authStoreStub.isLoggedIn = () => true;

    const result = run(() => authGuard({} as never, {} as never));

    expect(result).toBeTrue();
    expect(routerSpy.navigate).not.toHaveBeenCalled();
  });

  it('authGuard redirects to /login when logged out', () => {
    const result = run(() => authGuard({} as never, {} as never));

    expect(result).toBeFalse();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('staffGuard allows any staff role', () => {
    authStoreStub.isStaff = () => true;

    const result = run(() => staffGuard({} as never, {} as never));

    expect(result).toBeTrue();
  });

  it('staffGuard redirects a non-staff user to /products', () => {
    const result = run(() => staffGuard({} as never, {} as never));

    expect(result).toBeFalse();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/products']);
  });

  it('adminGuard allows only a full Admin', () => {
    authStoreStub.isAdmin = () => true;

    const result = run(() => adminGuard({} as never, {} as never));

    expect(result).toBeTrue();
  });

  it('adminGuard redirects a non-Admin (even staff) to /products', () => {
    authStoreStub.isStaff = () => true; // e.g. a Sub Admin - staff, but not full Admin

    const result = run(() => adminGuard({} as never, {} as never));

    expect(result).toBeFalse();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/products']);
  });
});
