import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {  UserClaims } from '../models/user-claims.model';
import { BehaviorSubject, Observable } from 'rxjs';
import { ResponseApi } from '../models/response-api.model';
import { parseJwt } from '../functions/DecodeJwt';
import { TokenData } from '../models/token-data.model';

@Injectable({
  providedIn: 'root'
})
export class SessionService {
  private usuarioSubject: BehaviorSubject<UserClaims>;
  private EMPTY!: UserClaims;
  constructor(private http: HttpClient) {
    this.usuarioSubject = new BehaviorSubject<UserClaims>(
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

  public getSessionUser(): Observable<ResponseApi<UserClaims>> {
    return this.http.get<ResponseApi<UserClaims>>(`Home/GetUserClaims`);
  }

  public get sessionData(): UserClaims {
    return this.usuarioSubject.value;
  }

  public createSessionUser(response: ResponseApi<UserClaims>): boolean {
    try {
      const user: UserClaims = response.data;
      sessionStorage.setItem("sessionUser", JSON.stringify(user));
      this.usuarioSubject.next(user);
      return true;
    } catch (error) {
      console.log(error)
      return false;
    }
  }

}
