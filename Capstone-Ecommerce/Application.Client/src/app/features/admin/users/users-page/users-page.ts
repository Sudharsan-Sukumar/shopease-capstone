import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { UserManagementService } from '../user-management.service';
import { AdminUser, Role } from '../user.model';
import { AuthStore } from '../../../auth/auth.store';

const PRIVILEGED_ROLES = ['Admin', 'SubAdmin'];

@Component({
  selector: 'app-users-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './users-page.html',
})
export class UsersPageComponent implements OnInit {
  private userService = inject(UserManagementService);
  private fb = inject(FormBuilder);
  private auth = inject(AuthStore);

  // Only Admin/Sub Admin can create users or edit roles at all; a Sub Admin
  // additionally can't touch anyone already Admin/Sub Admin, or grant those
  // roles - mirrors the same two-part check UserManagementService makes
  // server-side, so the UI never offers an action the API would reject.
  canManageUsers = this.auth.canManageUsers;
  isAdmin = this.auth.isAdmin;

  users = signal<AdminUser[]>([]);
  roles = signal<Role[]>([]);
  loading = signal(true);
  errorMessage = signal('');
  successMessage = signal('');

  // Create: Customer is never offered (self-registration only) - this panel
  // is for staff accounts. Edit: Customer stays available (an existing
  // user's Customer role is a legitimate thing to add/remove), just not
  // offered at creation time. Both hide Admin/Sub Admin from a Sub Admin's
  // checkboxes (no privilege escalation).
  createRoleOptions = computed(() =>
    this.roles().filter((r) => r.name !== 'Customer' && (this.isAdmin() || !PRIVILEGED_ROLES.includes(r.name))),
  );
  editRoleOptions = computed(() => this.roles().filter((r) => this.isAdmin() || !PRIVILEGED_ROLES.includes(r.name)));

  showCreateForm = signal(false);
  creating = signal(false);
  createRoles = signal<string[]>([]);

  createForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]],
    password: [
      '',
      [Validators.required, Validators.minLength(6), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$/)],
    ],
  });

  // Which row's role editor is open, and the checkbox state being edited for it.
  expandedUserId = signal<number | null>(null);
  editRoles = signal<string[]>([]);
  savingRoles = signal(false);
  revokingUserId = signal<number | null>(null);

  get f() {
    return this.createForm.controls;
  }

  ngOnInit(): void {
    this.loadUsers();
    // Roles come from the API, not a hardcoded list, so a role added later
    // shows up here without a frontend change - this is the "dynamic" part.
    // Skipped entirely for Supervisor/Support Agent - the API would 403 it
    // anyway since they can't create users or edit roles.
    if (this.canManageUsers()) {
      this.userService.getRoles().subscribe({
        next: (res) => {
          if (res.success && res.data) this.roles.set(res.data);
        },
      });
    }
  }

  /** Whether the current staff member is allowed to edit THIS user's roles - false for a Sub Admin looking at an Admin/Sub Admin row. */
  canEditUser(user: AdminUser): boolean {
    if (!this.canManageUsers()) return false;
    return this.isAdmin() || !user.roles.some((r) => PRIVILEGED_ROLES.includes(r));
  }

  /** Whether the current staff member is allowed to revoke THIS user's sessions - false for anyone below Admin looking at an Admin/Sub Admin row. */
  canRevokeUser(user: AdminUser): boolean {
    return this.isAdmin() || !user.roles.some((r) => PRIVILEGED_ROLES.includes(r));
  }

  private loadUsers(): void {
    this.loading.set(true);
    this.userService.getAll().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.users.set(res.data);
        else this.errorMessage.set(res.error?.message ?? 'Failed to load users.');
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load users. Is the API running?');
      },
    });
  }

  toggleCreateForm(): void {
    this.showCreateForm.update((v) => !v);
    if (!this.showCreateForm()) {
      this.createForm.reset();
      this.createRoles.set([]);
    }
  }

  toggleCreateRole(roleName: string, checked: boolean): void {
    this.createRoles.update((roles) => (checked ? [...roles, roleName] : roles.filter((r) => r !== roleName)));
  }

  submitCreate(): void {
    if (this.createForm.invalid || this.createRoles().length === 0) {
      this.createForm.markAllAsTouched();
      if (this.createRoles().length === 0) this.errorMessage.set('Select at least one role.');
      return;
    }

    this.creating.set(true);
    this.errorMessage.set('');
    const { fullName, email, phone, password } = this.createForm.getRawValue();
    this.userService
      .create({ fullName: fullName!, email: email!, phone: phone!, password: password!, roles: this.createRoles() })
      .subscribe({
        next: (res) => {
          this.creating.set(false);
          if (res.success && res.data) {
            this.users.update((list) => [...list, res.data!].sort((a, b) => a.email.localeCompare(b.email)));
            this.showCreateForm.set(false);
            this.createForm.reset();
            this.createRoles.set([]);
            this.flashSuccess(`User "${res.data.email}" created.`);
          } else {
            this.errorMessage.set(res.error?.message ?? 'Could not create user.');
          }
        },
        error: (err) => {
          this.creating.set(false);
          this.errorMessage.set(err.error?.error?.message ?? 'Could not create user.');
        },
      });
  }

  toggleEditRoles(user: AdminUser): void {
    if (this.expandedUserId() === user.id) {
      this.expandedUserId.set(null);
      return;
    }
    this.expandedUserId.set(user.id);
    this.editRoles.set([...user.roles]);
  }

  toggleEditRole(roleName: string, checked: boolean): void {
    this.editRoles.update((roles) => (checked ? [...roles, roleName] : roles.filter((r) => r !== roleName)));
  }

  saveRoles(user: AdminUser): void {
    if (this.editRoles().length === 0) {
      this.errorMessage.set('A user needs at least one role.');
      return;
    }
    this.savingRoles.set(true);
    this.userService.updateRoles(user.id, this.editRoles()).subscribe({
      next: (res) => {
        this.savingRoles.set(false);
        if (res.success && res.data) {
          this.users.update((list) => list.map((u) => (u.id === user.id ? res.data! : u)));
          this.expandedUserId.set(null);
          this.flashSuccess(`Roles updated for "${user.email}" - their current session is now invalid, they'll need to log in again.`);
        } else {
          this.errorMessage.set(res.error?.message ?? 'Could not update roles.');
        }
      },
      error: (err) => {
        this.savingRoles.set(false);
        this.errorMessage.set(err.error?.error?.message ?? 'Could not update roles.');
      },
    });
  }

  revokeSessions(user: AdminUser): void {
    this.revokingUserId.set(user.id);
    this.userService.revokeSessions(user.id).subscribe({
      next: (res) => {
        this.revokingUserId.set(null);
        if (res.success) this.flashSuccess(`All active sessions revoked for "${user.email}".`);
        else this.errorMessage.set(res.error?.message ?? 'Could not revoke sessions.');
      },
      error: (err) => {
        this.revokingUserId.set(null);
        this.errorMessage.set(err.error?.error?.message ?? 'Could not revoke sessions.');
      },
    });
  }

  private flashSuccess(message: string): void {
    this.successMessage.set(message);
    setTimeout(() => this.successMessage.set(''), 3500);
  }
}
