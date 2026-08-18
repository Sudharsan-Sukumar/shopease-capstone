import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

/**
 * UX-only gate - hides routes from a logged-out/non-admin UI. The REAL
 * security boundary is server-side ([Authorize(Roles=...)] on the API) -
 * a guard is client-side JS and trivially bypassable, so every protected
 * endpoint re-checks the role regardless of what this allowed through.
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthStore);
  const router = inject(Router);

  if (auth.isLoggedIn()) return true;

  router.navigate(['/login']);
  return false;
};

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthStore);
  const router = inject(Router);

  if (auth.isAdmin()) return true;

  router.navigate(['/products']);
  return false;
};

/** Any staff tier (Admin/Sub Admin/Supervisor/Support Agent) - the page itself hides create/edit controls for the lower tiers. */
export const staffGuard: CanActivateFn = () => {
  const auth = inject(AuthStore);
  const router = inject(Router);

  if (auth.isStaff()) return true;

  router.navigate(['/products']);
  return false;
};

/** Admin/Sub Admin only - unlike Users/Content, Supervisor/Support Agent have no product access at all, not even view. */
export const productManagerGuard: CanActivateFn = () => {
  const auth = inject(AuthStore);
  const router = inject(Router);

  if (auth.canManageProducts()) return true;

  router.navigate(['/products']);
  return false;
};
