import { PostHomeUserComponent } from "./post-home-user/post-home-user.component";
import { PostViewDetailUserComponent } from "./post-view-detail-user/post-view-detail-user.component";
import { UserContactComponent } from "./user-contact/user-contact.component";
import { PostViewUserComponent } from "./post-view-user/post-view-user.component";
import { BannerComponent } from "./banner/banner.component";
import { NavBarComponent } from "./nav-bar/nav-bar.component";
import { UserHomeRootingModule } from "./user-home-rooting.module";
import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { UserHomeComponent } from "./user-home.component";
import { AppModule } from "@app/app.module";
import { SharedModule } from "../shared/shared.module";
import { TabsModule } from "ngx-bootstrap/tabs";

@NgModule({
  declarations: [
    UserHomeComponent,
    NavBarComponent,
    BannerComponent,
    PostViewUserComponent,
    PostHomeUserComponent,
    UserContactComponent,
    PostViewDetailUserComponent, // Chi tiết phòng trọ
  ],
  imports: [
    CommonModule,
    UserHomeRootingModule,
    AppModule,
    SharedModule,
    TabsModule,
  ],
})
export class UserHomeModule {}
