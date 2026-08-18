export interface SalesReportRow {
  orderId: number;
  customerEmail: string;
  createdAtUtc: string;
  status: string;
  itemCount: number;
  total: number;
}

export interface SalesSummary {
  totalRevenue: number;
  orderCount: number;
  averageOrderValue: number;
  orders: SalesReportRow[];
}

export interface InventoryReportRow {
  productId: number;
  name: string;
  categoryName: string;
  price: number;
  stock: number;
  stockValue: number;
}

export interface InventoryReport {
  totalProducts: number;
  totalStockValue: number;
  products: InventoryReportRow[];
}
