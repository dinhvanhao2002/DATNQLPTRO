import { PostIntroUserComponent } from './post-intro-user/post-intro-user.component';
import { PostViewDetailUserComponent } from './post-view-detail-user/post-view-detail-user.component';
import { UserContactComponent } from './user-contact/user-contact.component';
import { RouterModule } from '@angular/router';
import { UserHomeComponent } from './user-home.component';
import { NgModule } from '@angular/core';
import { AppModule } from '@app/app.module';
import { AppRoutingModule } from '@app/app-routing.module';
import { PostViewUserComponent } from './post-view-user/post-view-user.component';
import { PostHomeUserComponent } from './post-home-user/post-home-user.component';



@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                component: UserHomeComponent,
                children: [
                    { path: '', component: PostHomeUserComponent },  // Trang chủ home
                    { path: 'post-vew-user', component: PostViewUserComponent },  // Trang Nhà cho thuê
                    { path: 'user-contact', component: UserContactComponent },  // Trang chủ Liên hệ
                    { path: 'post-view-detail/:id', component: PostViewDetailUserComponent },  // Trang chi tiết phòng trọ click từ trang home
                    { path: 'post-vew-user/post-view-detail/:id', component: PostViewDetailUserComponent },  // Trang chi tiết phòng trọ click từ trang nha cho thuê
                    { path: 'post-intro-user', component: PostIntroUserComponent },
                ]

            }

        ])
    ],
    exports: [
        RouterModule,
    ]
  })
  export class UserHomeRootingModule { }
