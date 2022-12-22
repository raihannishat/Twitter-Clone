import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'environments/environment';
import { delay, map, Observable } from 'rxjs';
import { UserViewModel } from './user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  baseUrl: string = environment.baseUrl + 'User/';

  constructor(private httpClient: HttpClient) { }

  getUserById(userId: string): Observable<UserViewModel> {

    return this.httpClient.get<UserViewModel>(this.baseUrl + `GetById/${userId}`);
  }


  checkIfEmailExists(value: string) {
    return this.getUserEmail(value).pipe(
      delay(500),
      map(result => (result ? { emailAlreadyExists: true } : null))
    );
  }

  getUserEmail(value: string): Observable<boolean> {
    const val = this.httpClient.get<boolean>(
      this.baseUrl + 'GetEmail' + '/' + value
    );
    // console.log(val);
    return val;
  }

  getUsersForSidebar(): Observable<boolean> {
    const val = this.httpClient.get<boolean>(
      this.baseUrl + 'GetAll?PageNumber=1'
    );
    return val;
  }
}
