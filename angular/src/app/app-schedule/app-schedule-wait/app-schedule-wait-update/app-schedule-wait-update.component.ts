import { Component, EventEmitter, Injector, OnInit, Output, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { CreateOrEditSchedulesDto, ManageAppointmentSchedulesServiceProxy, SessionServiceProxy } from '@shared/service-proxies/service-proxies';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { AppScheduleWaitComponent } from '../app-schedule-wait.component';
import { finalize } from 'rxjs/operators';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { TimepickerConfig } from 'ngx-bootstrap/timepicker';
import * as moment from 'moment';

@Component({
  selector: 'AppScheduleWaitUpdate',
  templateUrl: './app-schedule-wait-update.component.html',
  styleUrls: ['./app-schedule-wait-update.component.css'],
  providers : [ManageAppointmentSchedulesServiceProxy]
})
export class AppScheduleWaitUpdateComponent extends AppComponentBase implements OnInit {
  @ViewChild("editScheduleModal", { static: true }) modal: ModalDirective;
  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

  checkSave: boolean = false;
  active: boolean = false;
  saving: boolean = false;
  schedules: CreateOrEditSchedulesDto = new CreateOrEditSchedulesDto();
  tenantId: number;
  scheduleWaitComponent: AppScheduleWaitComponent;
  minDate: Date;
  timeString: string;

  constructor(
    injector: Injector,
    public _scheduleService: ManageAppointmentSchedulesServiceProxy,
    public _scheduleWaitComponent: AppScheduleWaitComponent,
    private bsDatepickerConfig: BsDatepickerConfig,
    private _sessionService: SessionServiceProxy,
    private timepickerConfig: TimepickerConfig,
  ) {
    super(injector);
    this.bsDatepickerConfig.dateInputFormat = 'DD/MM/YYYY';
    this.scheduleWaitComponent = _scheduleWaitComponent;
    this.minDate = new Date();
    this.minDate.getDate();

  }

  // ngOnInit(): void {
  //   this._sessionService.getCurrentLoginInformations().subscribe((res) => {
  //     this.tenantId = res.tenant.id;
  //   });
  // }

  // show(ScheduleId?: number): void {
  //   this._scheduleService
  //       .getScheduleForEdit(ScheduleId)
  //       .subscribe((result) => {
  //         this.schedules = result.createOrEditSchedulesDtos;
  //         this.active = true
  //         this.modal.show();
  //       });
  // }

  // save(): void {
  //   this.saving = true;
  //   this.schedules.tenantId = this.tenantId;
  //   this._scheduleService
  //     .updateSchedule(this.schedules)
  //     .pipe(
  //       finalize(() => {
  //         this.saving = false;
  //       })
  //     )
  //     .subscribe(() => {
  //       this.notify.info(this.l("SavedSuccessfully"));
  //       this.scheduleWaitComponent.updateTable();
  //       this.close();

  //       this.modalSave.emit();
  //       this.schedules = null;
  //       this.saving = false;
  //     });
  // }

  // close(): void {
  //   this.active = false;
  //   this.modal.hide();
  // }
  ngOnInit(): void {
    this._sessionService.getCurrentLoginInformations().subscribe((res) => {
      this.tenantId = res.tenant.id;
    });
  }

  show(ScheduleId?: number): void {
    this._scheduleService
      .getScheduleForEdit(ScheduleId)
      .subscribe((result) => {
        this.schedules = result.createOrEditSchedulesDtos;
        this.timeString = moment(this.schedules.hour).format('HH:mm');
        this.active = true;
        this.modal.show();
      });
  }

  save(): void {
    this.saving = true;
    // Combine the selected date with the time string to create a datetime
    this.schedules.hour = this.combineDateTime(this.schedules.day, this.timeString);
    this.schedules.tenantId = this.tenantId;
    this._scheduleService
      .updateSchedule(this.schedules)
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe(() => {
        this.notify.info(this.l("SavedSuccessfully"));
        this.scheduleWaitComponent.updateTable(); // Assuming this method exists in AppScheduleWaitComponent
        this.close(); // Close the modal

        this.modalSave.emit(); // Emit event to parent component
        this.schedules = new CreateOrEditSchedulesDto(); // Reset schedules object
      });
  }

  close(): void {
    this.active = false;
    this.modal.hide();
  }

  combineDateTime(date: any, time: string): moment.Moment {
    if (!date || !time) return null;

    const [hours, minutes] = time.split(':').map(Number);

    let combinedDateTime: moment.Moment;
    if (moment.isMoment(date)) {
      combinedDateTime = (date as moment.Moment).clone().hours(hours).minutes(minutes).seconds(0).milliseconds(0);
    } else {
      combinedDateTime = moment(date).hours(hours).minutes(minutes).seconds(0).milliseconds(0);
    }

    return combinedDateTime;
  }
}
