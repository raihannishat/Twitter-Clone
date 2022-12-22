import { Route } from '@angular/router';
import { HomeComponent } from 'app/modules/user/home/home.component';

export const homeRoutes: Route[] = [
    {
        path: '',
        component: HomeComponent
    },
    {
        path: ':tweetId/:userId',
        component: HomeComponent
    }
];
