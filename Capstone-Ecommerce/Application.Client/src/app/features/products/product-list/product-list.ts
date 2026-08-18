import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { ProductService } from '../product.service';
import { Category, Product } from '../product.model';
import { CartStore } from '../../cart/cart.store';
import { ContentService } from '../../admin/content/content.service';
import { ContentBlock } from '../../admin/content/content.model';

@Component({
  selector: 'app-product-list',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './product-list.html',
})
export class ProductListComponent implements OnInit {
  private productService = inject(ProductService);
  private cartService = inject(CartStore);
  private contentService = inject(ContentService);
  private destroyRef = inject(DestroyRef);

  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  banners = signal<ContentBlock[]>([]);
  loading = signal(true);
  searching = signal(false);
  errorMessage = signal('');
  addedMessage = signal('');

  // What the input box shows - updated on every keystroke, independent of
  // when a search request actually fires.
  search = signal('');

  categoryId = signal('');
  sortBy = signal('');
  minPrice = signal<number | null>(null);
  maxPrice = signal<number | null>(null);
  inStockOnly = signal(false);

  currentPage = signal(1);
  readonly perPage = 8;

  /**
   * Server-side search, properly debounced. Every keystroke pushes into
   * this Subject; `debounceTime` waits for a 300ms pause before doing
   * anything (no request per keystroke), `distinctUntilChanged` skips
   * firing again if the debounced value didn't actually change (e.g.
   * typing then deleting back to the same text), and `switchMap` cancels
   * whatever previous search request was still in flight the moment a new
   * one starts - so a fast typist can never have an old response overwrite
   * a newer one. Category/price/sort stay client-side `computed()` filters
   * on top of whatever `products` currently holds (see `filtered` below) -
   * only the free-text search is a real network round trip per term.
   */
  private searchInput$ = new Subject<string>();

  filtered = computed(() => {
    let items = this.products();

    if (this.categoryId()) items = items.filter((p) => p.categoryId === Number(this.categoryId()));
    if (this.minPrice() != null) items = items.filter((p) => p.price >= this.minPrice()!);
    if (this.maxPrice() != null) items = items.filter((p) => p.price <= this.maxPrice()!);
    if (this.inStockOnly()) items = items.filter((p) => p.stock > 0);

    const sort = this.sortBy();
    if (sort === 'price-asc') items = [...items].sort((a, b) => a.price - b.price);
    else if (sort === 'price-desc') items = [...items].sort((a, b) => b.price - a.price);
    else if (sort === 'name-asc') items = [...items].sort((a, b) => a.name.localeCompare(b.name));

    return items;
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.filtered().length / this.perPage)));

  // Clamps against totalPages here rather than resetting currentPage on every
  // filter change - one less signal write to coordinate.
  paged = computed(() => {
    const page = Math.min(this.currentPage(), this.totalPages());
    const start = (page - 1) * this.perPage;
    return this.filtered().slice(start, start + this.perPage);
  });

  ngOnInit(): void {
    this.productService.getCategories().subscribe({
      next: (res) => {
        if (res.success && res.data) this.categories.set(res.data);
      },
    });

    this.contentService.getActive().subscribe({
      next: (res) => {
        if (res.success && res.data) this.banners.set(res.data);
      },
    });

    this.loadProducts(); // initial full catalog load, no search term

    this.searchInput$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((term) => {
          this.searching.set(true);
          return this.productService.getAll(term || undefined);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (res) => {
          this.searching.set(false);
          this.currentPage.set(1);
          if (res.success && res.data) this.products.set(res.data);
        },
        error: () => this.searching.set(false),
      });
  }

  private loadProducts(): void {
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

  onSearchInput(term: string): void {
    this.search.set(term);
    this.searchInput$.next(term);
  }

  clearFilters(): void {
    this.search.set('');
    this.searchInput$.next('');
    this.categoryId.set('');
    this.sortBy.set('');
    this.minPrice.set(null);
    this.maxPrice.set(null);
    this.inStockOnly.set(false);
    this.currentPage.set(1);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
  }

  addToCart(product: Product): void {
    this.cartService.addItem(product.id, 1).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.cartService.setCart(res.data);
          this.addedMessage.set(`"${product.name}" added to cart.`);
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
