import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

// Models.
import { SessionUser, UserClaims } from '../models/user-claims.model';
import { BehaviorSubject, Observable } from 'rxjs';
import { ResponseApi } from '../models/response-api.model';
import { parseJwt } from '../functions/DecodeJwt';
import { TokenData } from '../models/token-data.model';

@Injectable({
  providedIn: 'root'
})
export class SessionService {
  private usuarioSubject: BehaviorSubject<SessionUser>;
  private EMPTY!: SessionUser;
  constructor(private http: HttpClient) {
    this.usuarioSubject = new BehaviorSubject<SessionUser>(
      JSON.parse(sessionStorage.getItem("sessionUser") ?? "{}")
    );
  }

  public getUserClaims() {
    const url = `Home/GetUserClaims/`;
      return this.http.get<UserClaims>(url);
  }

  public logout() {
    sessionStorage.removeItem("sessionUser");
    this.usuarioSubject.next(this.EMPTY);
    const url = `Auth/Logout/`;
    window.location.href = url;
  }

  public setSessionUser(): Observable<ResponseApi> {
    return this.http.get<ResponseApi>(`Home/GetTokenApiAndUserClaims`);
  }

  public get sessionData(): SessionUser {
    return this.usuarioSubject.value;
  }

  public createSessionUser(response: ResponseApi): boolean {
    try {
      const informationToken: TokenData = parseJwt(response.message);
      const user: SessionUser = {
        token: response.message,
        user: response.data,
        userId: informationToken.user,
        payrollId: informationToken.payrollId,
      };
      sessionStorage.setItem("sessionUser", JSON.stringify(user));
      this.usuarioSubject.next(user);
      return true;
    } catch (error) {
      return false;
    }
  }

}
