import { NgModule } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { FuseCardModule } from '@fuse/components/card';
import { SharedModule } from 'app/shared/shared.module';
import { PeopleRouting} from './people.routing';
import {PeopleComponent} from './people.component';
import {MatTabsModule} from "@angular/material/tabs";
import {MatButtonToggleModule} from "@angular/material/button-toggle";
import {AvatarModule} from "ngx-avatar";
import {UserComponent} from "../user/user.component";
import {MAT_DATE_FORMATS} from "@angular/material/core";
import * as moment from "moment/moment";
import {AuthService} from "../../../core/auth/auth.service";
import {HTTP_INTERCEPTORS} from "@angular/common/http";
import {AuthInterceptor} from "../../../core/auth/auth.interceptor";

@NgModule({
    declarations: [
        PeopleComponent, UserComponent
    ],
    imports: [
        RouterModule.forChild(PeopleRouting),
        MatFormFieldModule,
        MatInputModule,
        MatTooltipModule,
        FuseCardModule,
        SharedModule,
        MatTabsModule,
        MatButtonToggleModule,
        AvatarModule
    ],
    providers: [
        AuthService,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true
        }
    ]
})
export class PeopleModule {
}
