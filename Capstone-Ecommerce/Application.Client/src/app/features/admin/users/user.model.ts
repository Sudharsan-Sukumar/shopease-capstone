export interface AdminUser {
  id: number;
  email: string;
  fullName: string;
  phone: string;
  roles: string[];
  createdAtUtc: string;
}

export interface Role {
  id: number;
  name: string;
}
