import { HttpClient } from "@angular/common/http";
import { Component, Injector, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Route, Router } from "@angular/router";
import { AppComponentBase } from "@shared/app-component-base";
import {
  InforUserServiceProxy,
  ResetPasswordTokenDto,
} from "@shared/service-proxies/service-proxies";
import { finalize } from "rxjs/operators";

@Component({
  selector: "app-reset-password-token",
  templateUrl: "./reset-password-token.component.html",
  styleUrls: ["./reset-password-token.component.css"],
})
export class ResetPasswordTokenComponent
  extends AppComponentBase
  implements OnInit
{
  id: number;
  saving = false;

  resetPasswordForm!: FormGroup;
  emailToReset!: string;
  emailToken!: string;
  resetPasswordObj: ResetPasswordTokenDto = new ResetPasswordTokenDto();
  private baseUrl: string = "https://localhost:44311/api/User";

  constructor(
    injector: Injector,
    private userServiceProxy: InforUserServiceProxy,
    private fb: FormBuilder,
    private _activeRoute: ActivatedRoute,
    private _router: Router,
    private _http: HttpClient
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.resetPasswordForm = this.fb.group(
      {
        password: [
          null,
          [
            Validators.required,
            Validators.pattern('(?=^.{8,}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s)[0-9a-zA-Z!@#$%^&*()]*$')
          ]
        ],
        confirmPassword: [null, Validators.required],
      },
      {
        validator: this.ConfirmPasswordValidator("password", "confirmPassword"),
      }
    );

    this._activeRoute.queryParams.subscribe((val) => {
      this.emailToReset = val["email"];
      let uriToken = val["code"];
      this.emailToken = uriToken.replace(/ /g, "+");
      console.log(this.emailToken);
      console.log(this.emailToReset);
    });
  }

  reset() {
    if (this.resetPasswordForm.valid) {
      this.saving = true;
      this.resetPasswordObj.email = this.emailToReset;
      this.resetPasswordObj.newPassword = this.resetPasswordForm.value.password;
      this.resetPasswordObj.confirmPassword =
        this.resetPasswordForm.value.confirmPassword;
      this.resetPasswordObj.emailToken = this.emailToken;
      this._http
        .post(
          `${this.baseUrl}/reset-passwordToken`,this.resetPasswordObj
        )
        .pipe(finalize(() => (this.saving = false)))
        .subscribe(() => {
          this.notify.info(this.l("Cập nhật thành công"));
          this._router.navigate(['/']);
        });
    }
  }

  // Hàm confirm password validator
  ConfirmPasswordValidator(controlName: string, matchingControlName: string) {
    return (formGroup: FormGroup) => {
      const control = formGroup.controls[controlName];
      const matchingControl = formGroup.controls[matchingControlName];

      if (
        matchingControl.errors &&
        !matchingControl.errors.confirmPasswordValidator
      ) {
        return;
      }

      if (control.value !== matchingControl.value) {
        matchingControl.setErrors({ confirmPasswordValidator: true });
      } else {
        matchingControl.setErrors(null);
      }
    };
  }
}
