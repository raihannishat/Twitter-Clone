import { Route } from '@angular/router';
import { ProfileComponent } from 'app/modules/user/profile/profile.component';
import { NotificationsComponent } from '../notifications/notifications.component';

export const notificationRoutes: Route[] = [
    { path: '', component: NotificationsComponent } ,
];
