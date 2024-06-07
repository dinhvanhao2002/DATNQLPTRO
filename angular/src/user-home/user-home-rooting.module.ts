import { UserContactComponent } from './user-contact/user-contact.component';
import { RouterModule } from '@angular/router';
import { UserHomeComponent } from './user-home.component';
import { NgModule } from '@angular/core';
import { AppModule } from '@app/app.module';
import { AppRoutingModule } from '@app/app-routing.module';
import { PostViewUserComponent } from './post-view-user/post-view-user.component';



@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                component: UserHomeComponent,
                children: [
                    { path: '', component: PostViewUserComponent },  // Trang chủ home
                    { path: 'user-contact', component: UserContactComponent },  // Trang chủ home
                    { path: '', component: PostViewUserComponent },  // Trang chủ home
                    { path: '', component: PostViewUserComponent },  // Trang chủ home


                ]

            }

        ])
    ],
    exports: [
        RouterModule,
    ]
  })
  export class UserHomeRootingModule { }
