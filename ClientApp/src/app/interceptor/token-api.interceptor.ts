import { HttpRequest, HttpInterceptorFn } from '@angular/common/http';
import { inject } from "@angular/core";
import { SessionService } from "../services/session.service";
import { environment } from "../../environments/environment";

export const tokenApiInterceptor: HttpInterceptorFn = (req, next) => {
  const sessionService = inject(SessionService);
  const usuario = sessionService.sessionData;
  return next(req);
};


