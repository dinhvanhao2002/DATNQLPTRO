import { HttpClient } from '@angular/common/http';
import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppComponentBase } from '@shared/app-component-base';
import { EmailServiceServiceProxy, ResetPasswordTokenDto } from '@shared/service-proxies/service-proxies';
import { environment } from 'environments/environment';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
  providers: [EmailServiceServiceProxy]
})
export class ForgotPasswordComponent extends AppComponentBase {
  @ViewChild('createforgotPassword', {static: true}) modal: ModalDirective | undefined;

  resetPasswordEmail!: string;
  // biến check isvalid

  isValidEmail !: boolean;
  private baseUrl: string = "https://localhost:44311/api/User";
  resetPasswordDto: ResetPasswordTokenDto = new ResetPasswordTokenDto();
  isLoading : boolean = false;
  constructor(
    injector: Injector,
    private http: HttpClient,
    private _resetservice: EmailServiceServiceProxy,
   

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

  checkValidEmail(event: string): boolean {
    const pattern = /^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,3}$/;
    this.isValidEmail = pattern.test(event); // đúng định dạng
    return this.isValidEmail;
  }

  confirmToSend(){
    // đúng định dạng thì mới được gọi hàm kia vào
    if(this.checkValidEmail(this.resetPasswordEmail))
    {
      this.isLoading = true;
      console.log(this.resetPasswordEmail);
      this.http.post(`${this.baseUrl}/send-reset-email/${this.resetPasswordEmail}`,{}).pipe(finalize(() => this.isLoading = false)).subscribe(() =>{
        this.notify.info(this.l("Gửi email thành công"));
        this.close();
      })
    }
  }
}
