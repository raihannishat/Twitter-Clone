import { Route } from '@angular/router';
import {PeopleComponent} from './people.component';
import {UserComponent} from '../user/user.component';

export const PeopleRouting: Route[] = [
    { path: '', component: UserComponent } ,
    { path: ':id', component: PeopleComponent } ,
];
