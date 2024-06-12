
import { AgmMap } from '@agm/core';
import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'app-user-contact',
  templateUrl: './user-contact.component.html',
  styleUrls: ['./user-contact.component.css']
})
export class UserContactComponent extends AppComponentBase implements OnInit  {

  contactForm: FormGroup;
  lat: number = 10.8231;
  lng: number = 106.6297;
  zoom: number = 15;

  map: google.maps.Map;
  @ViewChild(AgmMap, { static: true }) agmMap: AgmMap;
  mapStyles: any = [
    {
      "featureType": "all",
      "elementType": "geometry",
      "stylers": [
        {
          "color": "#202c3e"
        }
      ]
    },
    // Add more styles as per your preference
  ];

  constructor(
    injector: Injector,
    private fb: FormBuilder

  ) {
    super(injector)
    this.contactForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      subject: ['', Validators.required],
      message: ['', Validators.required]
    });

  }
  ngOnInit(): void {
  }

  onSubmit(): void {
    if (this.contactForm.valid) {
      console.log(this.contactForm.value);
      // Gửi dữ liệu đến server hoặc xử lý form ở đây
    }
  }
}
