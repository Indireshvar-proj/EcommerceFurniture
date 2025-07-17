import { Component, OnInit, AfterViewInit } from '@angular/core';

declare var $: any; // declare jQuery

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, AfterViewInit {
  constructor() { }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    $('#tiva-slideshow').nivoSlider({
      pauseTime: 2000,    // time each slide stays (2 seconds)
      animSpeed: 500,     // transition speed
      pauseOnHover: false // slider won't pause on hover
    });
  }
}