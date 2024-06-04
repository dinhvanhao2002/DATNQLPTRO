import { Component, Injector, OnInit } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { AppAuthService } from '@shared/auth/app-auth.service';

@Component({
  selector: 'app-user-home',
  templateUrl: './user-home.component.html',
  styleUrls: ['./user-home.component.css']
})
export class UserHomeComponent extends AppComponentBase {

  showRoleId: number;
  showUserId: number;

  constructor(injector: Injector,
    private _authService: AppAuthService

  ) {
    super(injector);
   }

  ngOnInit() {

    this.showRoleId = this.appSession.getShownLoginRoleId();
    this.showUserId = this.appSession.getShownLoginId();

  }
  logout(): void {
    this._authService.logout();
  }


}
