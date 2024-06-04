import { Component, Injector, ViewChild } from '@angular/core';
import { AbpSessionService } from 'abp-ng2-module';
import { AppComponentBase } from '@shared/app-component-base';
import { accountModuleAnimation } from '@shared/animations/routerTransition';
import { AppAuthService } from '@shared/auth/app-auth.service';
import { ForgotPasswordComponent } from './Forgot-password/Forgot-password.component';

@Component({
  templateUrl: './login.component.html',
  animations: [accountModuleAnimation()]
})
export class LoginComponent extends AppComponentBase {

  @ViewChild('createforgotPassword', {static: true}) createforgotPassword: ForgotPasswordComponent | undefined;
  submitting = false;

  constructor(
    injector: Injector,
    public authService: AppAuthService,
    private _sessionService: AbpSessionService
  ) {
    super(injector);
  }

  get multiTenancySideIsTeanant(): boolean {
    return this._sessionService.tenantId > 0;
  }

  get isSelfRegistrationAllowed(): boolean {
    if (!this._sessionService.tenantId) {
      return false;
    }

    return true;
  }

  login(): void {
    this.submitting = true;
    this.authService.authenticate(() => (this.submitting = false));
  }
  openForgotPasswordModal(){
    
  }
}
