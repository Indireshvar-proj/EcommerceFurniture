import { Component, OnInit } from '@angular/core';
import { CheckOutService } from 'src/app/Services/checkOut.service';
import jsPDF from 'jspdf';
import { Product, OrderResponse, OrderItem } from 'src/app/Models/product.model';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-check-out',
  templateUrl: './check-out.component.html',
  styleUrls: ['./check-out.component.css']
})
export class CheckOutComponent implements OnInit {
  cartItems: Product[] = [];
  paymentInProgress = false;
  paymentSuccess = false;

  // Customer info bound to form inputs
  customerName: string = '';
  customerEmail: string = '';
  customerPhone: string = '';

  constructor(
    private checkOutService: CheckOutService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.loadCartItems();
  }

  private loadCartItems(): void {
    this.checkOutService.getCartItems().subscribe({
      next: (items: any[]) => {
        this.cartItems = items.map(item => ({
          ...item.product,
          quantity: item.quantity
        }));
        console.log('Checkout cart items:', this.cartItems);
      },
      error: (error) => {
        console.error('Error loading checkout items:', error);
      }
    });
  }

  get totalPrice(): number {
    return this.cartItems.reduce((sum, item) => sum + (item.price * (item.quantity || 1)), 0);
  }

  placeOrder(): void {
    this.paymentInProgress = true;
    this.paymentSuccess = false;

    this.checkOutService.placeOrder().subscribe({
      next: (response: OrderResponse) => {
        console.log('Order placed:', response);
        this.paymentSuccess = true;
        this.generateInvoicePDF(response);
        this.paymentInProgress = false;
      },
      error: (error) => {
        console.error('Error placing order:', error);
        this.paymentInProgress = false;
      }
    });
  }

  payNow(): void {
    if (!this.customerName || !this.customerEmail || !this.customerPhone) {
      alert('Please fill in your customer details before proceeding to payment.');
      return;
    }

    const paymentData = {
      CustomerName: this.customerName,
      Email: this.customerEmail,
      Phone: this.customerPhone,
      Amount: this.totalPrice
    };

    this.http.post<{ paymentUrl: string }>('https://localhost:7231/api/payments/create-order', paymentData).subscribe({
      next: (response) => {
        window.location.href = response.paymentUrl;
      },
      error: (error) => {
        console.error('Payment initiation failed', error);
        alert('Failed to initiate payment. Please try again.');
      }
    });
  }

  generateInvoicePDF(orderData: OrderResponse): void {
    const doc = new jsPDF();

    doc.setFontSize(18);
    doc.text('Invoice', 14, 22);

    if (!orderData) {
      doc.text('No order data available.', 14, 30);
      doc.save('invoice.pdf');
      return;
    }

    doc.setFontSize(12);
    doc.text(`Order ID: ${orderData.id || 'N/A'}`, 14, 40);
    doc.text(`Date: ${new Date(orderData.date || Date.now()).toLocaleDateString()}`, 14, 50);
    doc.text(`Customer: ${orderData.customerName || 'N/A'}`, 14, 60);

    doc.text('Item', 14, 75);
    doc.text('Qty', 90, 75);
    doc.text('Price', 120, 75);
    doc.text('Total', 150, 75);

    let y = 85;
    (orderData.items || []).forEach((item: OrderItem) => {
      doc.text(item.productName || '', 14, y);
      doc.text(String(item.quantity || 0), 90, y);
      doc.text(String(item.price || 0), 120, y);
      doc.text(String((item.quantity * item.price) || 0), 150, y);
      y += 10;
    });

    doc.text(`Grand Total: ${orderData.totalPrice || '0'}`, 14, y + 10);
    doc.save(`invoice_${orderData.id || 'unknown'}.pdf`);
    doc.output('dataurlnewwindow');
  }

  getImageUrl(item: Product): string {
    if (Array.isArray(item.images) && item.images.length > 0) {
      const firstImage = item.images[0];
      if (typeof firstImage === 'string') {
        return firstImage;
      } else if (typeof firstImage === 'object' && firstImage !== null && 'src' in firstImage) {
        return (firstImage as any).src;
      }
    }
    return 'assets/img/product/noImg.jpg';
  }
}
