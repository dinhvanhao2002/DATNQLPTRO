import { Component, OnInit } from '@angular/core';


@Component({
  selector: 'app-banner',
  templateUrl: './banner.component.html',
  styleUrls: ['./banner.component.css'],
  // animations: [
  //   trigger('slideInOut', [
  //     transition(':enter', [
  //       style({ transform: 'translateX(-100%)' }),
  //       animate('500ms ease-in', style({ transform: 'translateX(0%)' }))
  //     ]),
  //     transition(':leave', [
  //       animate('500ms ease-in', style({ transform: 'translateX(100%)' }))
  //     ])
  //   ])
  // ]
})
export class BannerComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
