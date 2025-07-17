export interface InvoiceItem {
    productName: string;
    quantity: number;
    price: number;
    total: number;
  }
  
  export interface InvoiceDto {
    orderId: number;
    userName: string;
    orderDate: string;  // or Date if you prefer and parse later
    totalPrice: number;
    items: InvoiceItem[];
  }