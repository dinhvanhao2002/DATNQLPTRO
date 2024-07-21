import { HttpClient } from '@angular/common/http';
import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { ReportInput } from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import * as saveAs from "file-saver";


@Component({
  selector: 'app-export-statistical-modal',
  templateUrl: './export-statistical-modal.component.html',
  styleUrls: ['./export-statistical-modal.component.css']
})
export class ExportStatisticalModalComponent extends AppComponentBase implements OnInit {
  @ViewChild("ExportStatistical", { static: true }) modal: ModalDirective;
  startDate: string;
  endDate: string;
  isLoading: boolean = false
  inputExport: ReportInput = new ReportInput();

  private baseUrl: string = "https://localhost:44311/api";
  form: any;


  constructor(
    injector: Injector,
    private http:  HttpClient

  ) {
    super(injector);
  }
  ngOnInit() {
  }

  show(){
    this.modal.show();

  }

  exportToExcel(){
    if (!this.inputExport.fromDate || !this.inputExport.toDate) {
      this.notify.warn(this.l("Vui lòng chọn ngày hợp lệ"));
      return;
    }

    this.isLoading = true;
    // const data = new ReportInput();
    // data.fromDate = moment(this.inputExport.fromDate);
    // data.toDate = moment(this.inputExport.toDate);

    let data = Object.assign(new ReportInput(),
    {
      fromDate: moment(this.inputExport.fromDate),
      toDate: moment(this.inputExport.toDate)
    });

    let fromDate = moment(this.inputExport.fromDate).format("DD/MM/YYYY");
    let toDate = moment(this.inputExport.toDate).format("DD/MM/YYYY");

    let fileName = "";
    fileName = `Báo cáo thống kê_${fromDate}_${toDate}.xlsx`;

    return this.http.post(`${this.baseUrl}/StatisticalReport/BpByDateReport`, data, { responseType: "blob" })
    .pipe(finalize(() => (this.isLoading = false)))
    .subscribe((blob) => {
        saveAs(blob, fileName);
        this.notify.success("Tải xuống thành công");
    });

  }

  close(){
    this.modal.hide();
  }

}


