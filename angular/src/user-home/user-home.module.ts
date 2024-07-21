import { HeaderUserHomeComponent } from './header-user-home/header-user-home.component';
import { PostHomeUserComponent } from "./post-home-user/post-home-user.component";
import { PostViewDetailUserComponent } from "./post-view-detail-user/post-view-detail-user.component";
import { UserContactComponent } from "./user-contact/user-contact.component";
import { PostViewUserComponent } from "./post-view-user/post-view-user.component";
import { UserHomeRootingModule } from "./user-home-rooting.module";
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { UserHomeComponent } from "./user-home.component";
import { AppModule } from "@app/app.module";
import { SharedModule } from "../shared/shared.module";
import { TabsModule } from "ngx-bootstrap/tabs";


@NgModule({
  declarations: [
    UserHomeComponent,
    PostViewUserComponent,
    PostHomeUserComponent,
    UserContactComponent,
    PostViewDetailUserComponent, // Chi tiết phòng trọ
    HeaderUserHomeComponent
  ],
  imports: [
    CommonModule,
    UserHomeRootingModule,
    AppModule,
    SharedModule,
    TabsModule,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class UserHomeModule {}
