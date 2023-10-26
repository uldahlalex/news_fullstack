import {Component} from "@angular/core";
import {FormControl, FormGroup, Validators} from "@angular/forms";
import {AlertController, ModalController, ToastController} from "@ionic/angular";
import {HttpClient, HttpErrorResponse} from "@angular/common/http";
import {environment} from "../../environments/environment";
import {DataService} from "../data.service";
import {firstValueFrom} from "rxjs";
import {Article} from "../models";
import {Router} from "@angular/router";

@Component({
  selector: 'edit-article-modal',
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-buttons>
          <ion-button color="danger" data-testid="delete_button" (click)="deleteArticleAlert()">Delete Article</ion-button>
        </ion-buttons>
        <ion-title>Edit Article</ion-title>
        <ion-buttons slot="end">

          <ion-button (click)="modalController.dismiss()">Close</ion-button>
        </ion-buttons>
      </ion-toolbar>
    </ion-header>
    <ion-content>
      <ion-item>
        <ion-input data-testid="edit_headline_form" [formControl]="headlineForm" label="Headline" labelPlacement="floating"></ion-input>
      </ion-item>
      <ion-item>
        <ion-input data-testid="edit_author_form" [formControl]="authorForm" label="Author" labelPlacement="floating"></ion-input>
      </ion-item>
      <ion-item>
        <ion-input data-testid="edit_body_form" [formControl]="bodyForm" label="Body" labelPlacement="floating"></ion-input>
      </ion-item>
      <ion-item>
        <ion-input data-testid="edit_img_form" [formControl]="articleImgUrlForm" label="Article image" labelPlacement="floating"></ion-input>
      </ion-item>

      <ion-button expand="full" data-testid="edit_submit_form" [disabled]="articleFormGroup.invalid" type="submit" (click)="editArticle()">Update
        article
      </ion-button>
    </ion-content>
  `
})
export class EditArticleComponent {

  headlineForm = new FormControl(this.dataService.currentArticle.headline, [Validators.required, Validators.minLength(5)]);
  authorForm = new FormControl(this.dataService.currentArticle.author, [Validators.required, Validators.pattern('(?:Bob|Rob|Dob|Lob)')]);
  bodyForm = new FormControl(this.dataService.currentArticle.body, [Validators.required]);
  articleImgUrlForm = new FormControl(this.dataService.currentArticle.articleImgUrl, [Validators.required]);
  articleFormGroup = new FormGroup({
    headline: this.headlineForm,
    author: this.authorForm,
    body: this.bodyForm,
    articleImgUrl: this.articleImgUrlForm
  })

  constructor(public modalController: ModalController,
              public alertController: AlertController,
              public http: HttpClient,
              public router: Router,
              public dataService: DataService,
              public toastController: ToastController) {

  }

  async deleteArticleAlert() {
    const alert = await this.alertController.create({
      message: 'Do you want to delete article?',
      buttons: [
        {
          role: "cancel",
          text: "No"
        },
        {
          role: "confirm",
          text: "Yes",
          handler: () => this.deleteArticle()
        }]
    })
    alert.present();

  }

  async deleteArticle() {
    try {
      const call = this.http.delete(environment.baseUrl + '/articles/' + this.dataService.currentArticle.articleId)
      const result = await firstValueFrom(call);
      this.dataService.articles = this.dataService.articles.filter(a => a.articleId != this.dataService.currentArticle.articleId);
      this.router.navigate(['']);
      this.modalController.dismiss()
      const toast = await this.toastController.create({
        message: 'Article has been deleted',
        duration: 1000,
        color: 'success'
      })
      toast.present();
    } catch (error:any) {
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
        duration: 2000,
        message: errorMessage
      });

      toast.present();
    }

  }

  async editArticle() {
    try {
      const call = this.http.put<Article>(environment.baseUrl + '/articles/' + this.dataService.currentArticle.articleId, this.articleFormGroup.value);
      const result = await firstValueFrom<Article>(call);
      let index = this.dataService.articles.findIndex(b => b.articleId == this.dataService.currentArticle.articleId)
      this.dataService.articles[index] = result;
      this.dataService.currentArticle = result;
      this.modalController.dismiss();
      const toast = await this.toastController.create({
        message: 'successfully updated',
        duration: 1000,
        color: 'success'
      })
      toast.present();

    } catch (error: any) {
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
        duration: 2000,
        message: errorMessage
      });

      toast.present();
    }

  }
}
