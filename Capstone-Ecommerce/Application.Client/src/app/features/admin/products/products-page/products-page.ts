import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProductManagementService } from '../product-management.service';
import { Category, Product } from '../../../products/product.model';

@Component({
  selector: 'app-products-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './products-page.html',
})
export class ProductsPageComponent implements OnInit {
  private productService = inject(ProductManagementService);
  private fb = inject(FormBuilder);

  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  loading = signal(true);
  errorMessage = signal('');
  successMessage = signal('');

  showForm = signal(false);
  editingId = signal<number | null>(null);
  saving = signal(false);
  togglingId = signal<number | null>(null);

  form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    price: [0, [Validators.required, Validators.min(0.01)]],
    stock: [0, [Validators.required, Validators.min(0)]],
    imageUrl: [''],
    categoryId: [0, [Validators.required, Validators.min(1)]],
  });

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    this.loadProducts();
    this.productService.getCategories().subscribe({
      next: (res) => {
        if (res.success && res.data) this.categories.set(res.data);
      },
    });
  }

  private loadProducts(): void {
    this.loading.set(true);
    this.productService.getAll().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.products.set(res.data);
        else this.errorMessage.set(res.error?.message ?? 'Failed to load products.');
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load products. Is the API running?');
      },
    });
  }

  toggleForm(): void {
    if (this.showForm()) {
      this.cancelForm();
      return;
    }
    this.editingId.set(null);
    this.form.reset({ name: '', description: '', price: 0, stock: 0, imageUrl: '', categoryId: 0 });
    this.showForm.set(true);
  }

  openEdit(product: Product): void {
    this.editingId.set(product.id);
    this.form.reset({
      name: product.name,
      description: product.description,
      price: product.price,
      stock: product.stock,
      imageUrl: product.imageUrl ?? '',
      categoryId: product.categoryId,
    });
    this.showForm.set(true);
  }

  cancelForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');
    const { name, description, price, stock, imageUrl, categoryId } = this.form.getRawValue();
    const payload = { name: name!, description: description ?? '', price: price!, stock: stock!, imageUrl: imageUrl || null, categoryId: categoryId! };

    const editingId = this.editingId();
    const existing = editingId ? this.products().find((p) => p.id === editingId) : null;
    const request = editingId
      ? this.productService.update(editingId, { ...payload, isActive: existing?.isActive ?? true })
      : this.productService.create(payload);

    request.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          const saved = res.data;
          if (editingId) {
            this.products.update((list) => list.map((p) => (p.id === editingId ? saved : p)));
            this.flashSuccess(`"${saved.name}" updated.`);
          } else {
            this.products.update((list) => [...list, saved].sort((a, b) => a.name.localeCompare(b.name)));
            this.flashSuccess(`"${saved.name}" added to the catalog.`);
          }
          this.cancelForm();
        } else {
          this.errorMessage.set(res.error?.message ?? 'Could not save product.');
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMessage.set(err.error?.error?.message ?? 'Could not save product.');
      },
    });
  }

  toggleActive(product: Product): void {
    this.togglingId.set(product.id);
    const nextActive = !product.isActive;

    const doToggle = (res: { success: boolean; data: Product | null; error: { message: string } | null }) => {
      this.togglingId.set(null);
      if (res.success) {
        this.products.update((list) => list.map((p) => (p.id === product.id ? { ...p, isActive: nextActive } : p)));
        this.flashSuccess(nextActive ? `"${product.name}" reactivated.` : `"${product.name}" deactivated.`);
      } else {
        this.errorMessage.set(res.error?.message ?? 'Could not update product.');
      }
    };

    if (nextActive) {
      // Reactivating - PUT with isActive:true, everything else unchanged.
      this.productService
        .update(product.id, {
          name: product.name,
          description: product.description,
          price: product.price,
          stock: product.stock,
          imageUrl: product.imageUrl,
          categoryId: product.categoryId,
          isActive: true,
        })
        .subscribe({ next: doToggle, error: () => this.togglingId.set(null) });
    } else {
      this.productService.deactivate(product.id).subscribe({ next: doToggle, error: () => this.togglingId.set(null) });
    }
  }

  private flashSuccess(message: string): void {
    this.successMessage.set(message);
    setTimeout(() => this.successMessage.set(''), 3500);
  }
}
