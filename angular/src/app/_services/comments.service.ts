import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from 'environments/environment';
import { Subject, BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CommentsService {

  public hubConnection: HubConnection;
  commentReceived = new Subject<any>();
  allCommentsReceived = new Subject<any>();
  updateComment = new Subject<any>();
  deleteComment = new Subject<any>();
  getCommentsForRent = new Subject<any>();
  getTotalComments = new BehaviorSubject<number>(0);

  // lấy ra thông báo thêm mới bài đăng của chủ trọ cho admin
  receiveNotification = new Subject<any>();

  // Lấy thông báo phê duyệt từ chủ trọ
  receiveNotificationForRent = new Subject<any>();

  // Lấy thông báo hủy bài đăng cho chủ trọ
  receiveNotificationCancelForRent = new Subject<any>();

  //chủ trọ
  receiveNotificationSchedule = new Subject<any>();

  baseUrl = environment.apiUrl;

  constructor() {
    this.startConnection();
  }

  public startConnection = () => {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.baseUrl}commentHub`)
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connection started');
        this.registerCommentEvents();
      })
      .catch(err => console.log('Error while starting connection: ' + err));
  }

  public registerCommentEvents = () => {
    this.hubConnection.on('ReceiveComment', (data) => {
      this.commentReceived.next(data);
    });

    this.hubConnection.on('UpdateComment', (data) => {
      this.updateComment.next(data);
    });

    this.hubConnection.on('DeleteComment', (data) => {
      this.deleteComment.next(data);
    });

    this.hubConnection.on('ReceiveAllComments', (data) => {
      this.allCommentsReceived.next(data);
    });

    this.hubConnection.on('GetTotalComments', (data: number) => {
      this.getTotalComments.next(data);;
    });

    this.hubConnection.on('GetCommentForRent', (data: number) => {  // chủ trọ
      this.getCommentsForRent.next(data);;
    });
    this.hubConnection.on('ReceiveNotification', (data) => {  // tới admin
      this.receiveNotification.next(data);;
    });

    this.hubConnection.on('ReceiveNotificationForRent', (data) => {  // tới chủ trọ
      this.receiveNotificationForRent.next(data);;
    });

    this.hubConnection.on('ReceiveNotificationCancelForRent', (data) => {  // tới chủ trọ về lí do hủy bài đăng
      this.receiveNotificationCancelForRent.next(data);;
    });

    this.hubConnection.on('ReceiveNotificationSchedule', (data) => {  // tới chủ trọ về lí do hủy bài đăng
      this.receiveNotificationSchedule.next(data);
    });
  }
}
