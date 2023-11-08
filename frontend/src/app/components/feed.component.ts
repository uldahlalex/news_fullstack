import {Component} from "@angular/core";
import {DataService} from "../data.service";
import {HttpClient} from "@angular/common/http";
import {firstValueFrom} from "rxjs";
import {ModalController} from "@ionic/angular";
import {CreateArticleComponent} from "./create-article.component";
import {ActivatedRoute, Router} from "@angular/router";
import {environment} from "../../environments/environment";
import {Article} from "../models";

@Component({
  template: `

    <ion-toolbar>
      <ion-buttons>

        <ion-input type="number" label-placement="floating" label="Resuls per page"
                   [(ngModel)]="resultsPerPage"></ion-input>

      </ion-buttons>
      <ion-title>welcome to the news app</ion-title>
      <ion-buttons slot="end">
        <ion-button data-testid="create_button" (click)="goToCreateArticle()">Add
          <ion-icon name="add-circle-outline"></ion-icon>
        </ion-button>
      </ion-buttons>
    </ion-toolbar>

    <ion-content class="ion-padding" fullscreen="true">
      <ion-card [attr.data-testid]="'card_'+article.headline" *ngFor="let article of dataService.articles"
                routerLink="/articles/{{article.articleId}}">
        <ion-toolbar>
          <ion-title>{{article.headline}}</ion-title>
        </ion-toolbar>
        <ion-card-subtitle>{{article.body}}</ion-card-subtitle>
      </ion-card>
      <ion-footer>
        <ion-button [disabled]="currentPage<2" (click)="previousPage()">Previous page</ion-button>
        <ion-button (click)="nextPage()">Next page</ion-button>
      </ion-footer>
    </ion-content>


  `,
})
export class FeedComponent {

  constructor(public dataService: DataService,
              public modalController: ModalController,
              public route: ActivatedRoute,
              public router: Router,
              public http: HttpClient) {
    this.getData();

  }

  resultsPerPage: number = 2;
  currentPage: number = 1;


  async goToCreateArticle() {
    const modal = await this.modalController.create({
      component: CreateArticleComponent
    })
    modal.present();
  }

  async nextPage() {
    this.currentPage = this.currentPage + 1;
    await this.router.navigate(['/'], {queryParams: {page: this.currentPage, resultsPerPage: this.resultsPerPage}});
    this.getData();

  }

  async previousPage() {
    this.currentPage = this.currentPage - 1;
    await this.router.navigate(['/'], {queryParams: {page: this.currentPage, resultsPerPage: this.resultsPerPage}});
     this.getData();
  }

  async getData() {
    const QueryParams = await firstValueFrom(this.route.queryParams);
    const page = QueryParams['page'] ?? 1
    const resultsPerPage = QueryParams['resultsPerPage'] ?? 2;
    this.currentPage = Number.parseInt(page) ?? 1;
    this.resultsPerPage = Number.parseInt(resultsPerPage) ?? 2;
    this.dataService.articles = await firstValueFrom<Article[]>(this.http.get<Article[]>(
      environment.baseUrl + "/feed?page=" + this.currentPage + "&resultsPerPage=" + this.resultsPerPage));
  }
}
