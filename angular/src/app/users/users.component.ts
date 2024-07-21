import { Component, Injector } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from 'shared/paged-listing-component-base';
import {
  UserServiceProxy,
  UserDto,
  UserDtoPagedResultDto
} from '@shared/service-proxies/service-proxies';
import { CreateUserDialogComponent } from './create-user/create-user-dialog.component';
import { EditUserDialogComponent } from './edit-user/edit-user-dialog.component';
import { ResetPasswordDialogComponent } from './reset-password/reset-password.component';
import Swal from 'sweetalert2';


class PagedUsersRequestDto extends PagedRequestDto {
  keyword: string;
  isActive: boolean | null;
}

@Component({
  templateUrl: './users.component.html',
  animations: [appModuleAnimation()]
})
export class UsersComponent extends PagedListingComponentBase<UserDto> {
  users: UserDto[] = [];
  keyword = '';
  isActive: boolean | null;
  advancedFiltersVisible = false;
  shownUserId = 2;

  constructor(
    injector: Injector,
    private _userService: UserServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }


  createUser(): void {
    this.showCreateOrEditUserDialog();
  }

  editUser(user: UserDto): void {
    this.showCreateOrEditUserDialog(user.id);
  }

  public resetPassword(user: UserDto): void {
    this.showResetPasswordUserDialog(user.id);
  }

  clearFilters(): void {
    this.keyword = '';
    this.isActive = undefined;
    this.getDataPage(1);
  }

  protected list(
    request: PagedUsersRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    request.isActive = this.isActive;

    this._userService
      .getAll(
        request.keyword,
        request.isActive,
        request.skipCount,
        request.maxResultCount
      )
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: UserDtoPagedResultDto) => {
        this.users = result.items;
        const userId = this.users.map(users => users.id == this.shownUserId);
        console.log(userId);
        console.log(this.users);
        this.showPaging(result, pageNumber);
      });
  }

  // this.message.confirm('', 'Bạn có chắc sẽ hủy ?', (isConfirme) => {

  // protected delete(user: UserDto): void {
  //   this.message.confirm(
  //    '','Bạn có thực sự muốn xóa người dùng này không ?',
  //     (result: boolean) => {
  //       if (result) {
  //         this._userService.delete(user.id).subscribe(() => {
  //           this.notify.success(this.l('SuccessfullyDeleted'));
  //           this.refresh();
  //         });
  //       }
  //     }
  //   );
  // }

  protected delete(user: UserDto): void {
    Swal.fire({
      //title: 'Xác nhận',
      text: 'Bạn có thực sự muốn xóa người dùng này không ?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Có',
      cancelButtonText: 'Không',
    }).then((result) => {
      if (result.isConfirmed) {
        this._userService.delete(user.id).subscribe(() => {
          Swal.fire('Đã xóa!', 'Người dùng đã được xóa thành công.', 'success');
          this.refresh();
        });
      }
    });
  }

  private showResetPasswordUserDialog(id?: number): void {
    this._modalService.show(ResetPasswordDialogComponent, {
      class: 'modal-lg',
      initialState: {
        id: id,
      },
    });
  }

  private showCreateOrEditUserDialog(id?: number): void {
    let createOrEditUserDialog: BsModalRef;
    if (!id) {
      createOrEditUserDialog = this._modalService.show(
        CreateUserDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    } else {
      createOrEditUserDialog = this._modalService.show(
        EditUserDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditUserDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
