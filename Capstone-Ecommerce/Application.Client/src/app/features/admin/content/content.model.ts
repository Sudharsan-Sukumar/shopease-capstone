export interface ContentBlock {
  id: number;
  key: string;
  title: string;
  body: string;
  imageUrl: string | null;
  isActive: boolean;
  displayOrder: number;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}
