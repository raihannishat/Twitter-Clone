import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FollowService } from 'app/core/follow/follow.service';
import { FollowersViewModel, FollowResponse } from 'app/modules/models/follow-model';
import { environment } from 'environments/environment';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss']
})
export class UserComponent implements OnInit {
  users: FollowersViewModel[];

  pageNumber = 1;

  baseUrl = environment.baseUrl + 'User/';
  constructor(
    private httpClient: HttpClient,
    private followService: FollowService,
    private router: Router
  ) { }

  ngOnInit(): void {
    const requestUrl = `${this.baseUrl}GetAll?PageNumber=1`;
    this.httpClient.get<FollowersViewModel[]>(requestUrl).subscribe((res) => {
      this.users = res;
    });
  }

  onScroll() {
    const requestUrl = `${this.baseUrl}GetAll?PageNumber=${++this.pageNumber}`;
    this.httpClient.get<FollowersViewModel[]>(requestUrl).subscribe((res) => {
      this.users.push(...res);
    });
  }

  onRedirectToProfile(userId: string): void {
    this.router.navigate(['/profile/', userId]);
  }

  follow(follow: FollowersViewModel) {
    this.followService.follow(follow.id).subscribe({
      next: (response: FollowResponse) => {

        let index = this.users.indexOf(follow);

        this.users[index].isFollowing = response.isFollowing;
        // debugger
      },
      error: (err) => {
        console.log(err);
      }
    });
  }
}
