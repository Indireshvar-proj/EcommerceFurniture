export interface Product {
  id: number;
  title_EN: string;
  title_AR: string;
  price: number;
  quantity?: number;
  images: { src: string; alt?: string }[];  // updated to objects with src
}


export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  price: number;
}

export interface OrderResponse {
  id: number;
  date: string;
  customerName: string;
  items: OrderItem[];
  totalPrice: number;
}
