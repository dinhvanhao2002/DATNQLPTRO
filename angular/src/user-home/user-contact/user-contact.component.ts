
import { AgmMap } from '@agm/core';
import { HttpClient } from '@angular/common/http';
import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { ContactFormModel } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';

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
  isLoading : boolean= false;

  contactEmail: ContactFormModel = new ContactFormModel();

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
  private baseUrl: string = "https://localhost:44311/api/User";

  constructor(
    injector: Injector,
    private fb: FormBuilder,
    private http: HttpClient,

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

  checkFormValidity(): boolean {
    let isValid = true;
    if (this.contactEmail.name.trim() === '') {
      this.notify.warn("Họ tên không được bỏ trống !")
      isValid = false;
    }

    if (this.contactEmail.email.trim() === '') {
      this.notify.warn("Họ tên không được bỏ trống !")
      isValid = false;
    }

    if (this.contactEmail.subject.trim() === '') {
      this.notify.warn("Họ tên không được bỏ trống !")

      isValid = false;
    }
    if (this.contactEmail.message.trim() === '') {
      this.notify.warn("Họ tên không được bỏ trống !")
      isValid = false;
    }
    return isValid;
  }

  onSubmit(): void {
    if(this.checkFormValidity()){
      this.isLoading = true;
      let data = Object.assign(new ContactFormModel(),
      {
        name: this.contactEmail.name,
        email:this.contactEmail.email,
        subject:this.contactEmail.subject,
        message:this.contactEmail.message
      });
      this.http.post(`${this.baseUrl}/send-contact-email}`,data).pipe(finalize(() => this.isLoading = false)).subscribe(() =>{
        this.notify.info(this.l("Gửi email thành công"));
      })
    }
  }
}
