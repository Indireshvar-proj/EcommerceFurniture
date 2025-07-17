import { Component, OnInit } from '@angular/core';
import { IWishlist } from 'src/app/Models/iwishlist';
import { WishlistService } from 'src/app/Services/wishlist.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-wishlist',
  templateUrl: './wishlist.component.html',
  styleUrls: ['./wishlist.component.css']
})
export class WishlistComponent implements OnInit {
  wishlist: IWishlist[] = [];

  constructor(private wishlistService: WishlistService) { }

  ngOnInit(): void {
    this.wishlistService.getWishList().subscribe({
      next: (wishlist) => {
        this.wishlist = wishlist;
        console.log('Wishlist loaded:', this.wishlist);
      },
      error: (err) => console.error('Error loading wishlist:', err)
    });
  }

  RemoveFromWishlist(prdId: number) {
    this.wishlistService.RemoveFromWishlist(prdId).subscribe({
      next: () => {
        console.log(`Product ${prdId} removed from wishlist.`);
      },
      error: (err) => console.error('Error removing product from wishlist:', err)
    });
  }

  getFullImageUrl(imageFileName?: string): string {
    if (!imageFileName || imageFileName.length === 0) {
      return 'assets/img/product/noImg.jpg'; // fallback local image
    }
    return `${environment.APIURL}/images/${imageFileName}`;
  }
}
