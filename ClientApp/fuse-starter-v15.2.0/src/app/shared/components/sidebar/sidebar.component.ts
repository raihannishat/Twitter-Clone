import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from '@angular/core';
import { environment } from 'environments/environment';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {

  hasTag: any;

  baseUrl = environment.baseUrl + 'Tweet/';
  constructor(
    private httpClient: HttpClient
  ) { }

  ngOnInit(): void {
    const requestUrl = `${this.baseUrl}Gethashtags?pageNumber=1`;
    this.httpClient.get<any>(requestUrl).subscribe((res) => {
      this.hasTag = res;
    });
  }
}
