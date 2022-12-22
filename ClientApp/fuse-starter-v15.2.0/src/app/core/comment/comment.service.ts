import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CommentResponse, CommentViewModel } from 'app/modules/models/comment-model';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class CommentService {

  baseUrl: string = environment.baseUrl + 'Comment/';

  constructor(private httpClient: HttpClient) { }

  createComment(comment): Observable<CommentResponse> {

    return this.httpClient.post<any>(this.baseUrl + 'Create', comment);
  }

  getComments(tweetId: string, pageNumber: number): Observable<CommentViewModel[]> {

    const requestUrl = `${this.baseUrl}Get?tweetId=${tweetId}&pageNumber=${pageNumber}`;

    return this.httpClient.get<CommentViewModel[]>(requestUrl);

  }

  deleteComment(tweetId: string, commentId: string): Observable<CommentResponse> {

    const requestUrl = `${this.baseUrl}Delete?tweetId=${tweetId}&commentId=${commentId}`;

    return this.httpClient.delete<CommentResponse>(requestUrl);
  }

}
