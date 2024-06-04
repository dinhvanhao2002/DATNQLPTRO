import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { ModalDirective } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent extends AppComponentBase {
  @ViewChild('createforgotPassword', {static: true}) modal: ModalDirective | undefined;

  resetPasswordEmail!: string;
  // biến check isvalid

  isValidEmail !: boolean;

  constructor(
    injector: Injector,

  ) {
    super(injector)

  }

  ngOnInit() {
  }


  show()
  {
    this.modal.show();
  }

  close()
  {
    this.modal.hide();
  }

  send(){

  }

  checkValidEmail(event: string) {
    const pattern = /^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,3}$/;
    this.isValidEmail = pattern.test(event); // đúng định dạng 

  }

}
