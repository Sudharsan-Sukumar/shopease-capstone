import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ContentService } from '../content.service';
import { ContentBlock } from '../content.model';
import { AuthStore } from '../../../auth/auth.store';

@Component({
  selector: 'app-content-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './content-page.html',
})
export class ContentPageComponent implements OnInit {
  private contentService = inject(ContentService);
  private fb = inject(FormBuilder);
  private auth = inject(AuthStore);

  // Same tier as product management (Admin/Sub Admin) - Supervisor/Support
  // Agent can view this page but the API would 403 any mutation from them.
  canManage = this.auth.canManageUsers;

  blocks = signal<ContentBlock[]>([]);
  loading = signal(true);
  errorMessage = signal('');
  successMessage = signal('');

  showForm = signal(false);
  editingId = signal<number | null>(null);
  saving = signal(false);
  deletingId = signal<number | null>(null);

  form = this.fb.group({
    key: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100), Validators.pattern(/^[a-z0-9-]+$/)]],
    title: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    body: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(1000)]],
    imageUrl: [''],
    isActive: [true],
    displayOrder: [0, [Validators.required]],
  });

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    this.loadBlocks();
  }

  private loadBlocks(): void {
    this.loading.set(true);
    this.contentService.getAll().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.blocks.set(res.data);
        else this.errorMessage.set(res.error?.message ?? 'Failed to load content blocks.');
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load content blocks. Is the API running?');
      },
    });
  }

  toggleForm(): void {
    if (this.showForm()) {
      this.cancelForm();
      return;
    }
    this.openCreate();
  }

  private openCreate(): void {
    this.editingId.set(null);
    this.form.reset({ key: '', title: '', body: '', imageUrl: '', isActive: true, displayOrder: this.blocks().length });
    this.f.key.enable();
    this.showForm.set(true);
  }

  openEdit(block: ContentBlock): void {
    this.editingId.set(block.id);
    this.form.reset({
      key: block.key,
      title: block.title,
      body: block.body,
      imageUrl: block.imageUrl ?? '',
      isActive: block.isActive,
      displayOrder: block.displayOrder,
    });
    this.f.key.disable(); // immutable once created - other placements may already reference it by key
    this.showForm.set(true);
  }

  cancelForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
    this.f.key.enable();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');
    const { key, title, body, imageUrl, isActive, displayOrder } = this.form.getRawValue();
    const payload = { title: title!, body: body!, imageUrl: imageUrl || null, isActive: isActive!, displayOrder: displayOrder! };

    const editingId = this.editingId();
    const request = editingId ? this.contentService.update(editingId, payload) : this.contentService.create({ key: key!, ...payload });

    request.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          const saved = res.data;
          if (editingId) {
            this.blocks.update((list) => list.map((b) => (b.id === editingId ? saved : b)).sort((a, b) => a.displayOrder - b.displayOrder));
            this.flashSuccess(`"${saved.title}" updated.`);
          } else {
            this.blocks.update((list) => [...list, saved].sort((a, b) => a.displayOrder - b.displayOrder));
            this.flashSuccess(`"${saved.title}" created.`);
          }
          this.cancelForm();
        } else {
          this.errorMessage.set(res.error?.message ?? 'Could not save content block.');
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMessage.set(err.error?.error?.message ?? 'Could not save content block.');
      },
    });
  }

  deleteBlock(block: ContentBlock): void {
    if (!confirm(`Delete "${block.title}"? This cannot be undone.`)) return;

    this.deletingId.set(block.id);
    this.contentService.delete(block.id).subscribe({
      next: (res) => {
        this.deletingId.set(null);
        if (res.success) {
          this.blocks.update((list) => list.filter((b) => b.id !== block.id));
          this.flashSuccess(`"${block.title}" deleted.`);
        } else {
          this.errorMessage.set(res.error?.message ?? 'Could not delete content block.');
        }
      },
      error: (err) => {
        this.deletingId.set(null);
        this.errorMessage.set(err.error?.error?.message ?? 'Could not delete content block.');
      },
    });
  }

  private flashSuccess(message: string): void {
    this.successMessage.set(message);
    setTimeout(() => this.successMessage.set(''), 3500);
  }
}
