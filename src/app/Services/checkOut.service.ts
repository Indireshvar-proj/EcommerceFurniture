import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { Product, OrderResponse } from '../Models/product.model';  // Adjust path if necessary
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CheckOutService {
  private baseURL: string;

  constructor(private http: HttpClient) {
    this.baseURL = environment.BaseUrl; // e.g. 'https://localhost:7231/api'
  }

  private getAuthHeaders(): HttpHeaders {
    // Assuming you store JWT or auth token in localStorage
    return new HttpHeaders({
      'Authorization': 'Bearer ' + (localStorage.getItem('access-token') || ''),
      'Content-Type': 'application/json'
    });
  }

  private handleError(error: any) {
    console.error('Checkout handling error:', error);
    return throwError(() => error);
  }

  // Get all products in cart
  getCartItems(): Observable<Product[]> {
    const url = `${this.baseURL}/Cart/GetAllProducts`;
    return this.http.get<Product[]>(url, { headers: this.getAuthHeaders() }).pipe(
      tap(data => console.log('Cart items:', data)),
      catchError(this.handleError)
    );
  }

  // Add product to cart or increase quantity by one
  addToCart(productId: number): Observable<number> {
    const url = `${this.baseURL}/Cart/AddToCart/${productId}`;
    return this.http.post<number>(url, null, { headers: this.getAuthHeaders() }).pipe(
      tap(count => console.log('Added to cart, new count:', count)),
      catchError(this.handleError)
    );
  }

  // Reduce quantity by one for a product in cart
  reduceQuantity(productId: number): Observable<number> {
    const url = `${this.baseURL}/Cart/${productId}`;
    return this.http.put<number>(url, null, { headers: this.getAuthHeaders() }).pipe(
      tap(count => console.log('Reduced quantity, new count:', count)),
      catchError(this.handleError)
    );
  }

  // Delete a product completely from cart
  removeFromCart(productId: number): Observable<number> {
    const url = `${this.baseURL}/Cart/${productId}`;
    return this.http.delete<number>(url, { headers: this.getAuthHeaders() }).pipe(
      tap(count => console.log('Removed from cart, new count:', count)),
      catchError(this.handleError)
    );
  }

  // Clear the entire cart
  clearCart(): Observable<boolean> {
    const url = `${this.baseURL}/Cart`;
    return this.http.delete<boolean>(url, { headers: this.getAuthHeaders() }).pipe(
      tap(result => console.log('Cart cleared:', result)),
      catchError(this.handleError)
    );
  }

  // In checkOut.service.ts
  placeOrder(): Observable<OrderResponse> {
    const url = `${this.baseURL}/CheckOut`;
    return this.http.post<OrderResponse>(url, {}, { headers: this.getAuthHeaders() }).pipe(
      tap(response => console.log('Order placed:', response)),
      catchError(this.handleError)
    );
  }
  
// Add this method inside CheckOutService class
initiatePayment(orderId: number): Observable<any> {
  const url = `${this.baseURL}/payments/create-order`; // note the 'payments' path as in your error
  return this.http.post<any>(url, { orderId }, { headers: this.getAuthHeaders() }).pipe(
    tap(response => console.log('Payment initiated:', response)),
    catchError(this.handleError)
  );
}


  
  
}
