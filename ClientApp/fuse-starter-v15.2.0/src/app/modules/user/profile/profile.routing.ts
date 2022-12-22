import { Route } from '@angular/router';
import { ProfileComponent } from 'app/modules/user/profile/profile.component';

export const profileRoutes: Route[] = [
    {
        path: '',
        component: ProfileComponent
    },
    {
        path: ':userId',
        component: ProfileComponent
    },
];
