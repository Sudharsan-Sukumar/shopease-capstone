export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stock: number;
  imageUrl: string | null;
  categoryName: string;
  categoryId: number;
  isActive: boolean;
}

export interface Category {
  id: number;
  name: string;
  slug: string;
}
