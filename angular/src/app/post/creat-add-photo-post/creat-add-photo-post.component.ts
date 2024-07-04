import {
  Component,
  EventEmitter,
  Injector,
  Input,
  OnInit,
  Output,
  ViewChild,
} from "@angular/core";
import { AppComponentBase } from "@shared/app-component-base";
import { Post } from "@shared/commom/models/post.model";
import {
  CreateOrEditIPostDto,
  FileParameter,
  ManagePostsServiceProxy,
  PhotoDto,
  SessionServiceProxy,
} from "@shared/service-proxies/service-proxies";
import { ModalDirective } from "ngx-bootstrap/modal";
import { FileUploader } from "ng2-file-upload";
import { environment } from "environments/environment";
import { finalize } from "rxjs/operators";
import { HttpClient } from "@angular/common/http";
import { PostComponent } from "../post.component";

@Component({
  selector: "app-creat-add-photo-post",
  templateUrl: "./creat-add-photo-post.component.html",
  styleUrls: ["./creat-add-photo-post.component.css"],
  providers: [ManagePostsServiceProxy],
})
export class CreatAddPhotoPostComponent extends AppComponentBase implements OnInit {
  @ViewChild("CreateAddPhoto", { static: true }) modal: ModalDirective;
  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
  @Input() post!: Post;

  active: boolean = false;
  saving: boolean = false;
  posts: CreateOrEditIPostDto = new CreateOrEditIPostDto();
  tenantId: number;
  uploader: FileUploader;
  baseUrl = environment.apiUrl;
  private postId: number;

  constructor(
    injector: Injector,
    private _postService: ManagePostsServiceProxy,
    private _sessionService: SessionServiceProxy,
    private http: HttpClient,
    public _postComponent: PostComponent,
  ) {
    super(injector);
    this.uploader = new FileUploader({
      url: `${this.baseUrl}api/services/app/ManagePosts/AddPhoto`,
      isHTML5: true,
      authToken: "Bearer " + abp.auth.getToken(),
      authTokenHeader: "Authorization",
      allowedFileType: ["image"],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024,
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const photo = JSON.parse(response);
        if (photo.isMain) {
          this.posts.photos = photo.url;
        }
      }
    };

  }

  ngOnInit() {
    this._sessionService.getCurrentLoginInformations().subscribe((res) => {
      this.tenantId = res.tenant.id;
    });
  }

  show(): void {
    this.posts = new CreateOrEditIPostDto();
    this.active = true;
    this.modal.show();
  }

  save(): void {
    this.saving = true;
    this.posts.tenantId = this.tenantId;
    this._postService.createOrEdit(this.posts)
      .pipe(finalize(() => {
        this.saving = false;
      }))
      .subscribe((result) => {
        this.postId = result;
        console.log(result);
        this.notify.info(this.l("SavedSuccessfully"));
        this.uploadImages();
        this.close();
        this._postComponent.updateTable();
      });
  }

  uploadImages(): void {
    const filesToUpload: FileParameter[] = this.uploader.queue.map(fileItem => {
      const fileParameter: FileParameter = {
        data: fileItem._file,
        fileName: fileItem.file.name
      };
      return fileParameter;
    });


    this._postService.createAndAddPhoto(this.postId, filesToUpload)
      .subscribe(() => {
        this.notify.success(this.l("Thêm ảnh thành công"));
      }, (error) => {
        this.notify.error(this.l("Thêm ảnh thất bại"));
      });
  }

  close(): void {
    this.active = false;
    this.modal.hide();
  }
}
