import {ModalController, ToastController} from "@ionic/angular";
import {DataService} from "../data.service";
import {FormControl, FormGroup, Validators} from "@angular/forms";
import {environment} from "../../environments/environment";
import {HttpClient, HttpErrorResponse} from "@angular/common/http";
import {firstValueFrom} from "rxjs";
import {Component} from "@angular/core";
import {Article} from "../models";

@Component({
  selector: 'new-article-modal',
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-title>New Article</ion-title>
        <ion-buttons slot="end">
          <ion-button (click)="modalController.dismiss()">Close</ion-button>
        </ion-buttons>
      </ion-toolbar>
    </ion-header>
    <ion-content>

      <ion-item>
        <ion-input  data-testid="create_headline_form" [formControl]="headlineForm" label="Headline" labelPlacement="floating"></ion-input>
      </ion-item>
      <ion-item>
        <ion-input  data-testid="create_author_form" [formControl]="authorForm" label="Author" labelPlacement="floating"></ion-input>
      </ion-item>
      <ion-item>
        <ion-input  data-testid="create_body_form" [formControl]="bodyForm" label="Body" labelPlacement="floating"></ion-input>
      </ion-item>
      <ion-item>
        <ion-input data-testid="create_img_form" [formControl]="articleImgUrlForm" label="Article image" labelPlacement="floating"></ion-input>
      </ion-item>


      <ion-button expand="full" data-testid="create_submit_form" [disabled]="articleFormGroup.invalid" type="submit" (click)="submitForm()">Insert
        article
      </ion-button>

    </ion-content>
  `
})
export class CreateArticleComponent {


  headlineForm = new FormControl('', [Validators.required, Validators.minLength(5)]);
  authorForm = new FormControl('', [Validators.required, Validators.pattern('(?:Bob|Rob|Dob|Lob)')]);
  bodyForm = new FormControl('', [Validators.required]);
  articleImgUrlForm = new FormControl('', [Validators.required]);

  articleFormGroup = new FormGroup({
    headline: this.headlineForm,
    author: this.authorForm,
    body: this.bodyForm,
    articleImgUrl: this.articleImgUrlForm
  })

  constructor(public modalController: ModalController,
              public dataService: DataService,
              public toastController: ToastController,
              public http: HttpClient) {
  }


  async submitForm() {
    try {
      const call = this.http.post<Article>(environment.baseUrl + '/articles', this.articleFormGroup.value)
      const result = await firstValueFrom<Article>(call);
      this.dataService.articles.push(result);
      const toast = await this.toastController.create({
        color: 'success',
        duration: 2000,
        message: "Success"
      })
      toast.present();
      this.modalController.dismiss();
    }catch (error: any) {
      console.log(error);
      let errorMessage = 'Error';

      if (error instanceof HttpErrorResponse) {
        // The backend returned an unsuccessful response code.
        errorMessage = error.error?.message || 'Server error';
      } else if (error.error instanceof ErrorEvent) {
        // A client-side or network error occurred.
        errorMessage = error.error.message;
      }

      const toast = await this.toastController.create({
        color: 'danger',
        duration: 200000,
        message: errorMessage
      });

      toast.present();
    }

  }
}
