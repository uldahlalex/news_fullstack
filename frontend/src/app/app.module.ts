import {NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';
import {Route, RouteReuseStrategy, RouterModule} from '@angular/router';
import {IonicModule, IonicRouteStrategy} from '@ionic/angular';
import {AppComponent} from './components/app.component';
import {FeedComponent} from "./components/feed.component";
import {HttpClientModule} from "@angular/common/http";
import {ArticleComponent} from "./components/article.component";
import {EditArticleComponent} from "./components/edit-article.component";
import {ReactiveFormsModule} from "@angular/forms";
import {CreateArticleComponent} from "./components/create-article.component";

const routes: Route[] = [
  {
    path: '',
    component: FeedComponent
  },
  {
    path: 'articles/:id',
    component: ArticleComponent
  }
]

@NgModule({
  declarations: [EditArticleComponent, CreateArticleComponent, AppComponent, FeedComponent, ArticleComponent],
  imports: [BrowserModule,
    IonicModule.forRoot({mode: "ios"}),
    RouterModule.forRoot(routes),
    HttpClientModule,
    ReactiveFormsModule
  ],
  providers: [{provide: RouteReuseStrategy, useClass: IonicRouteStrategy}],
  bootstrap: [AppComponent],
})
export class AppModule {
}
