import { SignalRService } from './../shared/helpers/signalr.service';
import { Component, Injector, OnInit } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { AppAuthService } from '@shared/auth/app-auth.service';
import { UserCommentServiceProxy, UserCommentViewDto } from '@shared/service-proxies/service-proxies';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-user-home',
  templateUrl: './user-home.component.html',
  styleUrls: ['./user-home.component.css'],
  providers: [
    UserCommentServiceProxy,SignalRService
  ],

})
export class UserHomeComponent extends AppComponentBase {

  showRoleId: number;
  showUserId: number;
  shownLoginName = '';
  shownLoginRoleId: number;
  shownLoginNameRole = '';
  unreadComments: UserCommentViewDto[] = [];
  private signalRSubscription: Subscription;
  constructor(
    injector: Injector,
    private _authService: AppAuthService,
    private _service: UserCommentServiceProxy,
    private signalRService: SignalRService // Inject SignalRService
  ) {
    super(injector);
  }

  ngOnInit() {
    this.showRoleId = this.appSession.getShownLoginRoleId();
    this.showUserId = this.appSession.getShownLoginId();

    this.shownLoginName = this.appSession.getShownLoginName();
    this.shownLoginRoleId = this.appSession.getShownLoginRoleId();
    this.updateLoginRoleName();

    this.getUnreadComments();

    this.subscribeToReceiveComment();
  }

  ngOnDestroy(): void {
    if (this.signalRSubscription) {
      this.signalRSubscription.unsubscribe();
    }
  }

  logout(): void {
    this._authService.logout();
  }

  private updateLoginRoleName(): void {
    switch (this.shownLoginRoleId) {
      case 3:
        this.shownLoginNameRole = 'Chủ trọ';
        break;
      case 4:
        this.shownLoginNameRole = 'Người thuê trọ';
        break;
      default:
        this.shownLoginNameRole = 'Admin';
        break;
    }
  }

  getUnreadComments(): void {
    this._service.getAllComment(this.showUserId).subscribe(
      (result: UserCommentViewDto[]) => {
        this.unreadComments = result;
      },
      (error) => {
        this.notify.error('Có lỗi xảy ra khi lấy bình luận chưa đọc');
      }
    );
  }

  private subscribeToReceiveComment(): void {
    this.signalRSubscription = this.signalRService.receiveComment().subscribe((comment: UserCommentViewDto) => {
      // Add the new comment to the unreadComments list
      this.unreadComments.unshift(comment); // Add to the beginning of the list
    });
  }
}
