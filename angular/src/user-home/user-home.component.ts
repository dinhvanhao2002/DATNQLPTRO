import { Component, HostListener, Injector, OnInit } from '@angular/core';
import { CommentsService } from '@app/_services/comments.service';
import { AppComponentBase } from '@shared/app-component-base';
import { AppAuthService } from '@shared/auth/app-auth.service';
import { ManagePostsServiceProxy, NotificationDto, NotificationScheduleNewDto, UserCommentServiceProxy, UserCommentViewDto, ManageAppointmentSchedulesServiceProxy } from '@shared/service-proxies/service-proxies';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-user-home',
  templateUrl: './user-home.component.html',
  styleUrls: ['./user-home.component.css'],
  providers: [
    UserCommentServiceProxy,
    ManagePostsServiceProxy,
    ManageAppointmentSchedulesServiceProxy
  ],

})
export class UserHomeComponent extends AppComponentBase {

  showRoleId: number;
  showUserId: number;
  shownLoginName = '';
  shownLoginRoleId: number;
  shownLoginNameRole = '';
  unreadComments: UserCommentViewDto[] = [];


 notifications: NotificationDto[] = [];
 notificationDtos: NotificationScheduleNewDto[] = [];

 unreadCount: number = 0;

 unreadSchedule: number = 0;

  constructor(
    injector: Injector,
    private _authService: AppAuthService,
    private _service: UserCommentServiceProxy,
    private _comment: CommentsService,
    private _servicePost: ManagePostsServiceProxy,
    private _serviceSchedule: ManageAppointmentSchedulesServiceProxy

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

    if(this.shownLoginRoleId == 2){
      this.showNotification();
      // Thông báo admin bài đăng mới
      this._comment.receiveNotification.subscribe((notification) => {
        this.notifications.push(notification);
        this.showNotification();
      });
    }

    if(this.shownLoginRoleId == 3)
    {

      // Thông báo tới chủ trọ
      this.showNotificationRent();

      this._comment.receiveNotificationForRent.subscribe((notification) => {
        this.notifications.push(notification);
        this.showNotificationRent();
      });
      // Thông báo tới cho chủ trọ bài đăng của bạn bị hủy
      this._comment.receiveNotificationCancelForRent.subscribe((notification) => {
        this.notifications.push(notification);
        this.showNotificationRent();
      });

      //Thông báo cho chủ trọ các lịch hẹn người thuê trọ hẹn
      this._comment.receiveNotificationSchedule.subscribe((notificationDtos) => {
        this.notificationDtos.push(notificationDtos);
        this.showNotificationRent();
      });

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

  // showNotification(comment) {
  //   // Thêm logic để hiển thị thông báo cho người dùng
  //   alert(`New comment from ${comment.userId}: ${comment.commentContent}`);
  // }

  markNotificationAsRead(notification: NotificationDto) {
    notification.isSending = true;
    this._servicePost.markNotificationAsRead(notification.id).subscribe(() => {
      this.checkUnreadNotifications();
    });
  }
  // Thông báo tới Admin
  showNotification() {
    this._servicePost.getNotifications().subscribe((notifications) => {
      this.notifications = notifications;
      this.checkUnreadNotifications();
    });
  }

  checkUnreadNotifications() {
    this.unreadCount = this.notifications.filter(notification => !notification.isSending ).length;
  }

  // Thông báo tới chủ trọ
  showNotificationRent()
  {
    this._servicePost.getLandlordNotificationsForApprovedPosts().subscribe((notifications) => {
      this.notifications = notifications;
      this.checkUnreadNotificationsForRent();
    });

    // thông báo lịch hẹn
    this._serviceSchedule.getNotificationHostSchedule().subscribe((notificationDtos) => {
      this.notificationDtos = notificationDtos;
      this.checkUnreadNotificationsSchedule();
    });
  }

  checkUnreadNotificationsForRent() {
    this.unreadCount = this.notifications.filter(notification => !notification.isSending ).length;
  }



  // Thông báo hủy bài đăng tới chủ trọ
  checkUnreadNotificationsSchedule(){
    this.unreadSchedule = this.notificationDtos.filter(notification => !notification.isSending ).length;
  }


}
