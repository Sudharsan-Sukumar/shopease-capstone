import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthStore } from '../auth.store';
import { CartStore } from '../../cart/cart.store';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthStore);
  private cartService = inject(CartStore);
  private router = inject(Router);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  errorMessage = signal('');
  submitting = signal(false);
  showPassword = signal(false);

  get f() {
    return this.form.controls;
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');
    this.submitting.set(true);

    this.auth.login(this.form.getRawValue() as { email: string; password: string }).subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.success && res.data) {
          this.auth.storeSession(res.data);
          this.cartService.refresh();
          // Staff land straight on the admin console, matching the ShopEase
          // prototype's separate admin experience - not the customer catalog.
          this.router.navigate([this.auth.isStaff() ? '/admin/dashboard' : '/products']);
        } else {
          this.errorMessage.set(res.error?.message ?? 'Login failed.');
        }
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(err.error?.error?.message ?? 'Login failed. Please try again.');
      },
    });
  }
}
