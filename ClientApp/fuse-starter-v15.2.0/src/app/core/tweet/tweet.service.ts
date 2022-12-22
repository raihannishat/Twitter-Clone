import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Tweet, TweetViewModel } from 'app/modules/models/tweet-model';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';


@Injectable({
	providedIn: 'root'
})
export class TweetService {
	tweet: Tweet = {
		content: ''
	};

	tweetVMList: TweetViewModel[];

	baseUrl: string = environment.baseUrl + 'Tweet/';
	tweets: Tweet[] = [];
	commnet: Comment[] = [];

	constructor(private _httpClient: HttpClient) { }

	createPost(tweet: Tweet): Observable<any> {
		return this._httpClient.post(this.baseUrl + 'Create', tweet);
	}

	getTweetById(tweetId: string, tweetOwnerId: string) {
		const requestUrl = `${this.baseUrl}GetById?tweetId=${tweetId}&tweetOwnerId=${tweetOwnerId}`;
		return this._httpClient.get<TweetViewModel>(requestUrl);
	}

	deletePost(id: string): Observable<any> {
		return this._httpClient.delete(this.baseUrl + 'Delete/' + id);
	}

	getUserTimelineData(userId: string, pageNumber: number): Observable<TweetViewModel[]> {

		const requestUrl = `${this.baseUrl}user-timeline?userId=${userId}&pageNumber=${pageNumber}`;

		return this._httpClient.get<TweetViewModel[]>(requestUrl);
	}

	getHomeTimelineData(pageNumber: number): Observable<TweetViewModel[]> {

		const requestUrl = `${this.baseUrl}home-timeline?pageNumber=${pageNumber}`;

		return this._httpClient.get<TweetViewModel[]>(requestUrl);
	}

	likeTweet() {

	}
}
