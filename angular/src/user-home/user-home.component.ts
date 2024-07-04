import { Component, HostListener, Injector, OnInit } from '@angular/core';
import { CommentsService } from '@app/_services/comments.service';
import { AppComponentBase } from '@shared/app-component-base';
import { AppAuthService } from '@shared/auth/app-auth.service';
import { UserCommentServiceProxy, UserCommentViewDto } from '@shared/service-proxies/service-proxies';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-user-home',
  templateUrl: './user-home.component.html',
  styleUrls: ['./user-home.component.css'],
  providers: [
    UserCommentServiceProxy
  ],

})
export class UserHomeComponent extends AppComponentBase {

  showRoleId: number;
  showUserId: number;
  shownLoginName = '';
  shownLoginRoleId: number;
  shownLoginNameRole = '';
  unreadComments: UserCommentViewDto[] = [];

  constructor(
    injector: Injector,
    private _authService: AppAuthService,
    private _service: UserCommentServiceProxy,
    private _comment: CommentsService

  ) {
    super(injector);
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.checkScroll();
  }

  checkScroll() {
    const button = document.getElementById('back-to-top');
    if (button) {
      if (window.pageYOffset > 300) {
        button.style.display = 'block';
      } else {
        button.style.display = 'none';
      }
    }
  }

  scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }


  ngOnInit() {
    this.showRoleId = this.appSession.getShownLoginRoleId();
    this.showUserId = this.appSession.getShownLoginId();

    this.shownLoginName = this.appSession.getShownLoginName();
    this.shownLoginRoleId = this.appSession.getShownLoginRoleId();
    this.updateLoginRoleName();

    //this.getUnreadComments();S
    this._comment.startConnection();

    this._comment.getCommentsForRent.subscribe((comment) => {
      this.unreadComments.push(comment);
      this.showNotification(comment);
    });

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

  showNotification(comment) {
    // Thêm logic để hiển thị thông báo cho người dùng
    alert(`New comment from ${comment.userId}: ${comment.commentContent}`);
  }


}
