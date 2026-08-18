export interface OrderItem {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
}

export interface Order {
  id: number;
  status: 'Confirmed' | 'PaymentPending';
  total: number;
  createdAtUtc: string;
  paymentTransactionId: string | null;
  items: OrderItem[];
}
