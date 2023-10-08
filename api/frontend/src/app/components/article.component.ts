import {Component} from "@angular/core";
import {HttpClient} from "@angular/common/http";
import {ActivatedRoute, Router} from "@angular/router";
import {firstValueFrom} from "rxjs";
import {ModalController} from "@ionic/angular";
import {Location} from "@angular/common";
import {DataService} from "../data.service";
import {environment} from "../../environments/environment";
import {EditArticleComponent} from "./edit-article.component";

@Component({
  template: `

      <ion-toolbar>
          <ion-buttons>
              <ion-button (click)="location.back()">
                  <ion-icon name="chevron-back-outline"></ion-icon>
              </ion-button>
          </ion-buttons>
          <ion-title>
              {{dataService.currentArticle.headline}}
          </ion-title>
          <ion-buttons slot="end">
              <ion-button data-testid="open_edit" (click)="openEditModal()">
                  <ion-icon name="cog-outline"></ion-icon>
              </ion-button>
          </ion-buttons>
      </ion-toolbar>

      <ion-content class="ion-padding" [fullscreen]="true">

          <ion-item>Author: {{dataService.currentArticle.author}}</ion-item>

          <ion-item>Body: {{dataService.currentArticle.body}}</ion-item>
          <ion-item>ID: {{dataService.currentArticle.articleId}}</ion-item>
          <ion-item>Image URL: {{dataService.currentArticle.articleImgUrl}}</ion-item>


      </ion-content>




  `,
})
export class ArticleComponent {

  constructor(public httpClient: HttpClient,
              public location: Location,
              public modalCtrl: ModalController,
              public dataService: DataService,
              public router: Router,
              public route: ActivatedRoute) {
    this.getArticle();
  }


  async getArticle() {
    try {
      const id = (await firstValueFrom(this.route.paramMap)).get('id');
      this.dataService.currentArticle = (await firstValueFrom(this.httpClient.get<any>(environment.baseUrl + '/articles/' + id)));

    } catch (e) {
      this.router.navigate(['']);
    }
  }

  async openEditModal() {
    const modal = await this.modalCtrl.create({
      component: EditArticleComponent,
    });
    modal.present();
  }
}
