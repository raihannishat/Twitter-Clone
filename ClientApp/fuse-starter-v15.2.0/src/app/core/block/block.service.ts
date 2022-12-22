import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BlockedUsersViewModel, BlockResponse } from 'app/modules/models/block-model';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BlockService {


  baseUrl: string = environment.baseUrl + 'Block/';

  constructor(private _httpClient: HttpClient) { }

  block(targetId: string): Observable<BlockResponse> {

    const requestUrl = `${this.baseUrl}block-user?userId=${targetId}`;

    return this._httpClient.post<BlockResponse>(requestUrl, {})

  }

  getBlockUsers(pageNumber: number): Observable<BlockedUsersViewModel[]> {

    const requestUrl = `${this.baseUrl}get-users?pageNumber=${pageNumber}`;

    return this._httpClient.get<BlockedUsersViewModel[]>(requestUrl);
  }

}
