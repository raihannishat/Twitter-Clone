import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LikeResponse } from 'app/modules/models/like-model';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class LikeService {

  baseUrl: string = environment.baseUrl + 'Like/';

  constructor(private _httpClient: HttpClient) { }

  likeTweet(tweetId: string, tweetCreatorId: string): Observable<LikeResponse> {

    const requestUrl = `${this.baseUrl}like-tweet`;

    return this._httpClient.post<LikeResponse>(requestUrl, { tweetId, tweetCreatorId })

  }
}
