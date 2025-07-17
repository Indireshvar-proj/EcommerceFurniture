import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, retry, tap } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { IWishlist } from '../Models/iwishlist';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private wishlistSubject = new BehaviorSubject<IWishlist[]>([]);
  wishlist$ = this.wishlistSubject.asObservable();

  constructor(private httpClient: HttpClient) {
    this.loadWishlist();
  }

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders().set('Authorization', 'Bearer ' + localStorage.getItem('access-token'));
  }

  private loadWishlist() {
    const headers = this.getAuthHeaders();
    this.httpClient.get<IWishlist[]>(`${environment.APIURL}/api/WishList`, { headers })
      .subscribe({
        next: data => this.wishlistSubject.next(data),
        error: err => console.error('Failed to load wishlist', err)
      });
  }

  getWishList(): Observable<IWishlist[]> {
    return this.wishlist$;
  }

  AddToWishlist(prdId: number): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.httpClient.post(`${environment.APIURL}/api/WishList/${prdId}`, null, { headers })
      .pipe(
        retry(2),
        catchError(this.handleError),
        tap(() => this.loadWishlist()) // Refresh wishlist after add
      );
  }

  RemoveFromWishlist(prdId: number): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.httpClient.delete(`${environment.APIURL}/api/WishList/${prdId}`, { headers })
      .pipe(
        catchError(this.handleError),
        tap(() => this.loadWishlist()) // Refresh wishlist after remove
      );
  }

  private handleError(error: HttpErrorResponse) {
    if (error.status === 0) {
      console.error('An error occurred:', error.error);
    } else {
      console.error(`Backend returned code ${error.status}, body was: `, error.error);
    }
    return throwError(() => new Error('Something bad happened; please try again later.'));
  }
}
