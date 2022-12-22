import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from 'environments/environment';
import { TweetViewModel } from '../../../modules/models/tweet-model';



@Injectable({
  providedIn: 'root'
})
export class SearchService {

  hashTweet: TweetViewModel[];
  baseUrl = environment.baseUrl + 'Tweet/';
  constructor(private httpClient: HttpClient, private router: Router, private activatedRoute: ActivatedRoute) { }

  getHashPost(value: string, pageNumber: number = 1) {
    const _value = (value.includes('#') ? value.slice(1) : value);
    const hashPost = `${this.baseUrl}GethashtagTweets?keyword=${_value}&pageNumber=${pageNumber}`;
    this.httpClient.get(hashPost)
      .subscribe((hashPostResult: TweetViewModel[]) => {
        if (pageNumber > 1) {
          hashPostResult.map((post) => {
            this.hashTweet.push(post);
          });
        }
        else {
          this.hashTweet = hashPostResult;
        }
        this.router.navigate(['/post'], { queryParams: { search: _value } });

      });
  }

}
