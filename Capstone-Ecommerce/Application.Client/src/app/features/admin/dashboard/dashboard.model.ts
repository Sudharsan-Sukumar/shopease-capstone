export interface LowStockProduct {
  id: number;
  name: string;
  stock: number;
}

export interface RecentOrder {
  id: number;
  customerName: string;
  itemCount: number;
  total: number;
  status: string;
  createdAtUtc: string;
}

export interface MonthlyRevenuePoint {
  month: string;
  revenue: number;
  orderCount: number;
}

export interface DashboardSummary {
  totalRevenue: number;
  totalOrders: number;
  pendingOrders: number;
  averageOrderValue: number;
  totalCustomers: number;
  totalProducts: number;
  inStockProducts: number;
  outOfStockProducts: number;
  lowStockProducts: LowStockProduct[];
  recentOrders: RecentOrder[];
  monthlyRevenue: MonthlyRevenuePoint[];
}
