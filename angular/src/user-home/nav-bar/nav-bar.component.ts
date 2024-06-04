import { Component, Injector, OnInit } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { AppAuthService } from '@shared/auth/app-auth.service';

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.css']
})
export class NavBarComponent extends AppComponentBase {

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
