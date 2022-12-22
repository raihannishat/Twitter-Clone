import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, Observable, switchMap, take, throwError } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    /**
     * Constructor
     */
    // constructor(private _authService: AuthService) {
    // }

    // /**
    //  * Intercept
    //  *
    //  * @param req
    //  * @param next
    //  */
    // intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    //     // Clone the request object
    //     let newReq = req.clone();

    //     // Request
    //     //
    //     // If the access token didn't expire, add the Authorization header.
    //     // We won't add the Authorization header if the access token expired.
    //     // This will force the server to return a "401 Unauthorized" response
    //     // for the protected API routes which our response interceptor will
    //     // catch and delete the access token from the local storage while logging
    //     // the user out from the app.


    //     // if (this._authService.accessToken && AuthUtils.isTokenExpired(this._authService.accessToken)) {
    //     //     this._authService.getRefreshToken().subscribe({
    //     //         next: (res) => {
    //     //             if (!!res.refreshToken) {
    //     //                 this._authService.accessToken = res.accessToken;
    //     //             }
    //     //             else {
    //     //                 localStorage.removeItem('accessToken');
    //     //             }
    //     //         },
    //     //         error: (error) => {
    //     //             console.log(error);
    //     //         }
    //     //     })
    //     // }

    //     if (this._authService.accessToken && !AuthUtils.isTokenExpired(this._authService.accessToken)) {
    //         newReq = req.clone({
    //             headers: req.headers.set('Authorization', 'Bearer ' + this._authService.accessToken)
    //         });
    //     }

    //     // Response
    //     return next.handle(newReq).pipe(
    //         catchError((error) => {

    //             // Catch "401 Unauthorized" responses
    //             if (error instanceof HttpErrorResponse && error.status === 401) {
    //                 // Sign out
    //                 this._authService.signOut();

    //                 // Reload the app
    //                 // location.reload();
    //             }

    //             return throwError(error);
    //         })
    //     );
    // }


    // ----------------------------------------------------------




    refreshTokenModel = {
        accessToken: this.authService.accessToken,
        refreshToken: this.authService.refreshToken,
    };
    private isRefreshing = false;
    private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(
        null
    );
    constructor(private authService: AuthService, private _router: Router,) { }
    intercept(
        req: HttpRequest<any>,
        next: HttpHandler
    ): Observable<HttpEvent<Object>> {
        let authReq = req;
        const accessToken = this.authService.accessToken;

        if (!!accessToken) {
            authReq = this.addTokenHeader(req, accessToken);
        }

        return next.handle(authReq).pipe(
            catchError((error) => {
                if (
                    error instanceof HttpErrorResponse
                    // !authReq.url.includes('Auth/signin') 
                    &&
                    error.status === 401
                ) {
                    return this.handle401Error(authReq, next);
                }
                return throwError(error);
            })
        );
    }
    private handle401Error(request: HttpRequest<any>, next: HttpHandler) {
        if (!this.isRefreshing) {
            this.isRefreshing = true;
            this.refreshTokenSubject.next(null);
            this.refreshTokenModel = {
                accessToken: this.authService.accessToken,
                refreshToken: this.authService.refreshToken,
            };

            if (this.refreshTokenModel.refreshToken) {
                return this.updateAccessToken(next, request);
            }
        }
        return this.refreshTokenSubject.pipe(
            filter((accessToken) => accessToken !== null),
            take(1),
            switchMap((accessToken) => next.handle(this.addTokenHeader(request, accessToken)))
        );
    }
    private updateAccessToken(next: HttpHandler, request: HttpRequest<any>) {
        return this.authService.getRefreshToken().pipe(
            switchMap((result: any) => {
                this.isRefreshing = false;
                this.authService.accessToken = result.accessToken;
                this.authService.refreshToken = result.refreshToken;
                this.refreshTokenSubject.next(result.accessToken);
                return next.handle(this.addTokenHeader(request, result.accessToken));
            }),
            catchError((err) => {
                this.isRefreshing = false;
                // this.authService.signOut();
                this._router.navigate(['/sign-out']);
                return throwError(err);
            })
        );
    }

    private addTokenHeader(request: HttpRequest<any>, accessToken: string) {
        return request.clone({
            headers: request.headers.set('Authorization', 'Bearer ' + accessToken),
        });
    }
}
