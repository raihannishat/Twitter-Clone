import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { FuseCardModule } from '@fuse/components/card';
import { SharedModule } from 'app/shared/shared.module';
import { NotificationsComponent } from '../notifications/notifications.component';
import { notificationRoutes } from './notifications.routing';
import {AvatarModule} from "ngx-avatar";

@NgModule({
    declarations: [
        NotificationsComponent
    ],
    imports: [
        RouterModule.forChild(notificationRoutes),
        MatFormFieldModule,
        MatInputModule,
        MatTooltipModule,
        FuseCardModule,
        SharedModule,
        AvatarModule
    ]
})
export class NotificationsModule {
}
