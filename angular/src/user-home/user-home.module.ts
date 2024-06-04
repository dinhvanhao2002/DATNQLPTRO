import { BannerComponent } from './banner/banner.component';
import { NavBarComponent } from './nav-bar/nav-bar.component';
import { UserHomeRootingModule } from './user-home-rooting.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserHomeComponent } from './user-home.component';

@NgModule({
  imports: [
    CommonModule,
    UserHomeRootingModule
  ],
  declarations: [
    UserHomeComponent,
    NavBarComponent,
    BannerComponent

  ]
})
export class UserHomeModule { }
