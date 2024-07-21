import { Component, Injector, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { CancelPostDto, ConfirmPackageDto, GetPackageViewDto, PackagePostsServiceProxy } from '@shared/service-proxies/service-proxies';
import { AppPackagePostsVipProComponent } from './app-package-posts-vip-pro/app-package-posts-vip-pro.component';
import { AppPackagePostsVipComponent } from './app-package-posts-vip/app-package-posts-vip.component';
import { PaginationParamsModel } from '@shared/commom/models/base.model';
import { ceil } from 'lodash-es';
import { Table } from 'primeng/table';
import { AppPackagePostsEditComponent } from './app-package-posts-edit/app-package-posts-edit.component';
import Swal from 'sweetalert2';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { ActivatedRoute } from '@angular/router';


@Component({
  selector: 'app-package-posts',
  templateUrl: './app-package-posts.component.html',
  styleUrls: ['./app-package-posts.component.css'],
  providers: [PackagePostsServiceProxy]
})
export class AppPackagePostsComponent extends AppComponentBase implements OnInit {


  @ViewChild('AppPackagePostsVipPro') AppPackagePostsVipPro: AppPackagePostsVipProComponent;
  @ViewChild('AppPackagePostsVip') AppPackagePostsVip: AppPackagePostsVipComponent;
  @ViewChild('AppPackagePostsEdit') AppPackagePostsEdit: AppPackagePostsEditComponent;

  isAdmin: boolean = false;
  shownLogin: number;
  rowDataPackage: GetPackageViewDto[];
  rowData: GetPackageViewDto[];
  filterText;
  sorting: string = "";
  paginationParams: PaginationParamsModel;
  pagination: PaginationParamsModel;
  selectedPackage:any;
  maxResultCount: number = 20;
  packageConfirm: ConfirmPackageDto = new ConfirmPackageDto();
  packageCancel: CancelPostDto = new CancelPostDto();
  tenantId: number;
  statusPackage: boolean = false;
  isLoading = false;
  active: boolean = false;

  @ViewChild('tabset', { static: false }) tabset: TabsetComponent;
  @ViewChildren(TabDirective) tabs: QueryList<TabDirective>;

  constructor(
    injector: Injector,
    public _packageService: PackagePostsServiceProxy,
    private activeroute: ActivatedRoute
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.shownLogin = this.appSession.getShownLoginRoleId();
     // check nếu là chủ trọ thì RoId = 3
    // nếu là admin thì roid = 2
    console.log(this.shownLogin);
    if (this.shownLogin == 2) {
      this.isAdmin = true;
    } else {
      this.isAdmin = false;
    }

    // this.rowDataPackage = [];
    // this.paginationParams = { pageNum: 1, pageSize: 20, totalCount: 0 };
    // this.getAll(this.paginationParams).subscribe(data => {
    //   console.log(data.items);
    //   this.rowDataPackage = data.items;
    //   this.paginationParams.totalPage = ceil(data.totalCount / this.maxResultCount);
    //   this.paginationParams.totalCount = data.totalCount;
    // });
    this.getStatus();
    this.updateTable();

    this.selectSecondTab();


  }

  getStatus(): void {
    this._packageService.statusConfirm(this.packageConfirm).subscribe((
      res => {
        this.statusPackage = res;
      }
    ))
  }

  getAll(paginationParams: PaginationParamsModel) {
    return this._packageService.getAll(
      this.filterText,
      this.sorting ?? null,
      paginationParams ? paginationParams.skipCount : 0,
      paginationParams ? paginationParams.pageSize : 20,

    );
  }

  getAllForHost(paginationParams: PaginationParamsModel) {
    return this._packageService.getAllForHost(
      this.filterText,
      this.sorting ?? null,
      paginationParams ? paginationParams.skipCount : 0,
      paginationParams ? paginationParams.pageSize : 20,

    );
  }

  clear(table: Table) {
    table.clear();
  }

  createPackageVIPpro(): void {
    this.AppPackagePostsVipPro.show();
  }

  createPackageVIP(): void {
    this.AppPackagePostsVip.showVip();
  }

    // Xác nhận gói đăng ký
  confirmPackage() {
    this.getPackageConfirm(this.selectedPackage.id);
    console.log(this.selectedPackage);
  }

  getPackageConfirm(PackageId?: number): void {
    this._packageService
        .getPackageForEdit(PackageId)
        .subscribe((result) => {
          this.packageConfirm = result.confirmPackageDtos;
          this.active = true;
          this.confirm();
        });
  }

  // confirm(): void {
  //   this.getStatus();
  //   this.message.confirm('', 'Bạn có chắc sẽ xác nhận ?', (isConfirme) => {
  //     if (isConfirme) {
  //       if (this.statusPackage) {
  //         this.notify.warn("Gói bài đã được xác nhận");
  //       } else  {
  //         this.packageConfirm.tenantId = this.tenantId;
  //         this._packageService.confirmPackage(this.packageConfirm)
  //           .subscribe(() => {
  //             this.notify.success('Gói này đã được lưu');
  //             this.updateTable();
  //           })
  //       }
  //     }
  //   })
  // }
  confirm(): void {
    this.getStatus();

    Swal.fire({
      title: 'Xác nhận',
      text: 'Bạn có chắc sẽ xác nhận ?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Có',
      cancelButtonText: 'Hủy bỏ',
    }).then((result) => {
      if (result.isConfirmed) {
        if (this.statusPackage) {
          this.notify.warn('Gói bài đã được xác nhận');
        } else {
          this.packageConfirm.tenantId = this.tenantId;
          this._packageService.confirmPackage(this.packageConfirm).subscribe(() => {
            this.notify.success('Gói này đã được lưu');
            this.updateTable();
          });
        }
      }
    });
  }


  // Hủy gói đăng ký
  cancelPackage() {
    this.getPackageCancel(this.selectedPackage.id);
    console.log(this.selectedPackage);
  }

  getPackageCancel(PackageId?: number): void {
    this._packageService
        .getPackageForEdit(PackageId)
        .subscribe((result) => {
          this.packageCancel = result.cancelPostDtos;
          this.active = true;
          this.cancel();
        });
  }

  // cancel(): void {
  //   this.message.confirm('', 'Bạn có chắc sẽ hủy ?', (isConfirme) => {
  //     if (isConfirme) {
  //       this.packageCancel.tenantId = this.tenantId;
  //       this._packageService.cancelPackage(this.packageCancel)
  //         .subscribe(() => {
  //           this.notify.success('Gói này đã được hủy');
  //           this.updateTable();
  //         })
  //     }
  //   })
  // }
  cancel(): void {
    Swal.fire({
      title: 'Xác nhận',
      text: 'Bạn có chắc sẽ hủy ?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Có',
      cancelButtonText: 'Hủy bỏ',
    }).then((result) => {
      if (result.isConfirmed) {
        this.packageCancel.tenantId = this.tenantId;
        this._packageService.cancelPackage(this.packageCancel).subscribe(() => {
          this.notify.success('Gói này đã được hủy');
          this.updateTable();
        });
      }
    });
  }

  updateTable() {
    this.isLoading = true;
    this.rowDataPackage = [];
    this.paginationParams = { pageNum: 1, pageSize: 20, totalCount: 0 };
    this.getAll(this.paginationParams).subscribe(data => {
      console.log(data.items);
      this.rowDataPackage = data.items;
      this.paginationParams.totalPage = ceil(data.totalCount / this.maxResultCount);
      this.paginationParams.totalCount = data.totalCount;
      this.isLoading = false;
    });
    this.rowData = [];
    this.pagination = { pageNum: 1, pageSize: 20, totalCount: 0 };
    this.getAllForHost(this.pagination).subscribe(data => {
      console.log(data.items);
      this.rowData = data.items;
      this.pagination.totalPage = ceil(data.totalCount / this.maxResultCount);
      this.pagination.totalCount = data.totalCount;
      this.isLoading = false;
    });
  }

  editPackage() {
    this.AppPackagePostsEdit.showPackage(this.selectedPackage.id);
    console.log(this.selectedPackage);
  }

  deletePackage() {
    this.message.confirm('', 'Bạn có thực sự muốn hủy gói này ?', (isConfirme) => {
      if (isConfirme) {
        this._packageService.deletePackage(this.selectedPackage.id)
          .subscribe(() => {
            this.notify.success('Gói đăng bài đã được hủy');
            this.updateTable();
          })
      }
    })
  }

  selectSecondTab(): void {
    if (this.tabset) {
      this.tabset.tabs[1].active = true; // Index 1 corresponds to the second tab
    }
  }



}
