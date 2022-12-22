import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { FollowersViewModel, FollowingsViewModel, FollowResponse } from 'app/modules/models/follow-model';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';


@Injectable({
	providedIn: 'root'
})
export class FollowService {


	baseUrl: string = environment.baseUrl + 'Follow/';

	constructor(private _httpClient: HttpClient) { }

	follow(targetId: string): Observable<FollowResponse> {

		const requestUrl = `${this.baseUrl}id/?targetId=${targetId}`;

		return this._httpClient.post<FollowResponse>(requestUrl, {})

	}

	getFollowersList(userId: string, pageNumber: number): Observable<FollowersViewModel[]> {

		const requestUrl = `${this.baseUrl}follower-list?userId=${userId}&pageNumber=${pageNumber}`;

		return this._httpClient.get<FollowersViewModel[]>(requestUrl);
	}

	getFollowingsList(userId: string, pageNumber: number): Observable<FollowingsViewModel[]> {

		const requestUrl = `${this.baseUrl}following-list?userId=${userId}&pageNumber=${pageNumber}`;

		return this._httpClient.get<FollowingsViewModel[]>(requestUrl);
	}
}
