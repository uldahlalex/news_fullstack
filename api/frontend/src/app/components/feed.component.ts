import {Component} from "@angular/core";
import {DataService} from "../data.service";
import {HttpClient} from "@angular/common/http";
import {firstValueFrom} from "rxjs";
import {environment} from "../../environments/environment";
import {ModalController} from "@ionic/angular";
import {CreateArticleComponent} from "./create-article.component";
import {Article} from "../models";

@Component({
  template: `

      <ion-toolbar>
          <ion-title>welcome to the news app</ion-title>
          <ion-buttons slot="end">
              <ion-button data-testid="create_button" (click)="goToCreateArticle()">Add
                  <ion-icon name="add-circle-outline"></ion-icon>
              </ion-button>
          </ion-buttons>
      </ion-toolbar>

      <ion-content class="ion-padding" fullscreen="true">
          <ion-card [attr.data-testid]="'card_'+article.headline" *ngFor="let article of dataService.articles" routerLink="/articles/{{article.articleId}}">
              <ion-toolbar>
                  <ion-title>{{article.headline}}</ion-title>
              </ion-toolbar>
              <ion-card-subtitle>{{article.body}}</ion-card-subtitle>
          </ion-card>
      </ion-content>


  `,
})
export class FeedComponent {
  constructor(public dataService: DataService,
              public modalController: ModalController,
              public http: HttpClient) {
    this.getFeedData();
  }

  async getFeedData() {
    const call = this.http.get<Article[]>(environment.baseUrl + '/feed');
    this.dataService.articles = await firstValueFrom<Article[]>(call);
  }

  async goToCreateArticle() {
    const modal = await this.modalController.create({
      component: CreateArticleComponent
    })
    modal.present()
  }
}
