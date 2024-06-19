import { filter } from 'rxjs/operators';
import { Component, Injector, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppComponentBase } from '@shared/app-component-base';
import { PackagePostDto, PackagePostsServiceProxy, SessionServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
  selector: 'app-app-package-posts-payment-success',
  templateUrl: './app-package-posts-payment-success.component.html',
  styleUrls: ['./app-package-posts-payment-success.component.css'],
  providers: [PackagePostsServiceProxy]
})
export class AppPackagePostsPaymentSuccessComponent extends AppComponentBase implements OnInit {
  paymentUrl: any;

  packages: PackagePostDto = new PackagePostDto();
  vnp_TransactionStatus: string = "";
  vnp_Amount: string = "";
  vnp_OrderInfo: string = "";
  vnp_ReturnUrl: string = "http://localhost:4200/app/app-package-posts-payment-success";
  constructor(
    injector: Injector,
    public _packageService: PackagePostsServiceProxy,
    private _sessionService: SessionServiceProxy,
    private _router: Router,
    private route: ActivatedRoute
  ) {
    super(injector);
  }

  ngOnInit(): void {
    // Gọi hàm thanh toán khi component được khởi tạo
    // this.createPackageAndPay();
    this.route.queryParamMap.subscribe(params => {
      this.vnp_TransactionStatus = params.get('vnp_TransactionStatus') || '';
      this.vnp_Amount = params.get('vnp_Amount') || '';
      this.vnp_OrderInfo = params.get('vnp_OrderInfo') || '';
      //this.vnp_ReturnUrl = params.get('vnp_ReturnUrl') || '';



      console.log(this.vnp_TransactionStatus);
      console.log(this.vnp_Amount);
      console.log(this.vnp_OrderInfo);
      console.log(this.vnp_ReturnUrl);


    });
    this.callback();

  }


  callback(){
    if (this.vnp_TransactionStatus == "00") {
      this.packages.packageType = 'Gói VIP pro';
      this.packages.paymentUrl = this.vnp_ReturnUrl;
      console.log(this.paymentUrl);
      this._packageService.callBack(this.packages).subscribe((res)=>{

        this.notify.info('Thanh toán thành công');
      })
    }
  }


  returnToPackagePosts(): void {
    this._router.navigate(['../app-package-posts']);
  }
}
