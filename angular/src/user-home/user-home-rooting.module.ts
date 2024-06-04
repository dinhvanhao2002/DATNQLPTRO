import { RouterModule } from '@angular/router';
import { UserHomeComponent } from './user-home.component';
import { NgModule } from '@angular/core';


@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                component: UserHomeComponent

            }
        ])
    ],
    exports: [
        RouterModule

    ]
  })
  export class UserHomeRootingModule { }
