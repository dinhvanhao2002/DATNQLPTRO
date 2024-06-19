import { Injectable } from '@angular/core';
import { UserCommentViewDto } from './../service-proxies/service-proxies';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';



export class SignalRService {
  private hubConnection: HubConnection;
  private receivedCommentSubject: Subject<UserCommentViewDto> = new Subject<UserCommentViewDto>();

  constructor() {
    this.createConnection();
    this.registerOnServerEvents();
    this.startConnection();
  }

  private createConnection() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('/signalr') // Replace with your SignalR Hub URL if different
      .build();
  }

  private startConnection(): void {
    this.hubConnection.start()
      .then(() => console.log('SignalR connection started'))
      .catch(err => console.error('Error while starting SignalR connection: ' + err));
  }

  private registerOnServerEvents(): void {
    this.hubConnection.on('ReceiveComment', (comment: UserCommentViewDto) => {
      this.receivedCommentSubject.next(comment);
    });
  }

  receiveComment(): Observable<UserCommentViewDto> {
    return this.receivedCommentSubject.asObservable();
  }
}
