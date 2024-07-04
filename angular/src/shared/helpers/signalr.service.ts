
// import { Subject } from 'rxjs';

// @Injectable({
//   providedIn: 'root'
// })
// export class SignalRService {
//   private hubConnection: signalR.HubConnection;
//   public commentReceived: Subject<any> = new Subject<any>();

//   constructor() {
//     this.startConnection();
//     this.addCommentListener();
//   }

//   private startConnection() {
//     this.hubConnection = new signalR.HubConnectionBuilder()
//       .withUrl('https://your-api-url/hub') // Thay thế bằng URL của SignalR Hub trên server của bạn
//       .build();

//     this.hubConnection
//       .start()
//       .then(() => console.log('SignalR connection started'))
//       .catch(err => console.log('Error while starting connection: ' + err));
//   }

//   private addCommentListener() {
//     this.hubConnection.on('ReceiveComment', (data) => {
//       this.commentReceived.next(data);
//     });
//   }
// }
