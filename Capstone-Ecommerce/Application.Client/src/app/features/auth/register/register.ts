import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthStore } from '../auth.store';
import { CartStore } from '../../cart/cart.store';

function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirm = control.get('confirmPassword')?.value;
  return password && confirm && password !== confirm ? { mismatch: true } : null;
}

@Component({
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthStore);
  private cartService = inject(CartStore);
  private router = inject(Router);

  form = this.fb.group(
    {
      fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]],
      password: [
        '',
        [Validators.required, Validators.minLength(6), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$/)],
      ],
      confirmPassword: ['', Validators.required],
    },
    { validators: passwordsMatch },
  );

  errorMessage = signal('');
  submitting = signal(false);
  showPassword = signal(false);
  showConfirm = signal(false);

  get f() {
    return this.form.controls;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');
    this.submitting.set(true);

    const { fullName, email, phone, password } = this.form.getRawValue();
    this.auth.register({ fullName: fullName!, email: email!, phone: phone!, password: password! }).subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.success && res.data) {
          this.auth.storeSession(res.data);
          this.cartService.setCart({ items: [], total: 0 }); // brand-new user, always starts empty
          this.router.navigate(['/products']);
        } else {
          this.errorMessage.set(res.error?.message ?? 'Registration failed.');
        }
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(err.error?.error?.message ?? 'Registration failed. Please try again.');
      },
    });
  }
}
