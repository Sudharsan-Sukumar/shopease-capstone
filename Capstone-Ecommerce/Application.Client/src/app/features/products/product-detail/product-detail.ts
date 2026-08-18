import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProductService } from '../product.service';
import { Product } from '../product.model';
import { CartStore } from '../../cart/cart.store';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './product-detail.html',
})
export class ProductDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private cartService = inject(CartStore);

  product = signal<Product | null>(null);
  loading = signal(true);
  notFound = signal(false);
  quantity = signal(1);
  addedMessage = signal('');

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.productService.getById(id).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.product.set(res.data);
        else this.notFound.set(true);
      },
      error: () => {
        this.loading.set(false);
        this.notFound.set(true);
      },
    });
  }

  changeQty(delta: number): void {
    const stock = this.product()?.stock ?? 1;
    this.quantity.update((v) => Math.min(Math.max(1, v + delta), stock));
  }

  addToCart(): void {
    const product = this.product();
    if (!product) return;

    this.cartService.addItem(product.id, this.quantity()).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.cartService.setCart(res.data);
          this.addedMessage.set(`Added ${this.quantity()} to cart.`);
        } else {
          this.addedMessage.set(res.error?.message ?? 'Could not add to cart.');
        }
        setTimeout(() => this.addedMessage.set(''), 2500);
      },
      error: (err) => {
        this.addedMessage.set(err.error?.error?.message ?? 'Could not add to cart.');
        setTimeout(() => this.addedMessage.set(''), 2500);
      },
    });
  }
}
