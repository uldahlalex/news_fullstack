import {Injectable} from "@angular/core";
import {Article} from "./models";

@Injectable({
  providedIn: 'root'
})
export class DataService {

  public articles: Article[] = [];
  public currentArticle: Article = {};
}


