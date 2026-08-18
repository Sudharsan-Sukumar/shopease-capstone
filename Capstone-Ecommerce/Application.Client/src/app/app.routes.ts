import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { authGuard, productManagerGuard, staffGuard } from './features/auth/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'products',
    canActivate: [authGuard],
    loadComponent: () => import('./features/products/product-list/product-list').then((m) => m.ProductListComponent),
  },
  {
    path: 'products/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/products/product-detail/product-detail').then((m) => m.ProductDetailComponent),
  },
  {
    path: 'cart',
    canActivate: [authGuard],
    loadComponent: () => import('./features/cart/cart-page/cart-page').then((m) => m.CartPageComponent),
  },
  {
    path: 'checkout',
    canActivate: [authGuard],
    loadComponent: () => import('./features/checkout/checkout-page/checkout-page').then((m) => m.CheckoutPageComponent),
  },
  {
    path: 'orders',
    canActivate: [authGuard],
    loadComponent: () => import('./features/orders/orders-page/orders-page').then((m) => m.OrdersPageComponent),
  },
  {
    path: 'admin/products',
    canActivate: [authGuard, productManagerGuard],
    loadComponent: () => import('./features/admin/products/products-page/products-page').then((m) => m.ProductsPageComponent),
  },
  {
    path: 'admin/users',
    canActivate: [authGuard, staffGuard],
    loadComponent: () => import('./features/admin/users/users-page/users-page').then((m) => m.UsersPageComponent),
  },
  {
    path: 'admin/dashboard',
    canActivate: [authGuard, staffGuard],
    loadComponent: () => import('./features/admin/dashboard/dashboard-page/dashboard-page').then((m) => m.DashboardPageComponent),
  },
  {
    path: 'admin/content',
    canActivate: [authGuard, staffGuard],
    loadComponent: () => import('./features/admin/content/content-page/content-page').then((m) => m.ContentPageComponent),
  },
  {
    path: 'admin/reports',
    canActivate: [authGuard, staffGuard],
    loadComponent: () => import('./features/admin/reports/reports-page/reports-page').then((m) => m.ReportsPageComponent),
  },
  { path: '**', redirectTo: 'login' },
];
