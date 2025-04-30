import { HttpRequest, HttpInterceptorFn } from '@angular/common/http';
import { inject } from "@angular/core";
import { SessionService } from "../services/session.service";
import { environment } from "../../environments/environment";

export const tokenApiInterceptor: HttpInterceptorFn = (req, next) => {
  const sessionService = inject(SessionService);
  const usuario = sessionService.sessionData;

  // Condición para agregar el token
  if (usuario !== undefined
    && JSON.stringify(usuario) !== '{}'
    && usuario?.token !== "" && usuario.token !== "" && req.url.includes(environment.urlApi)) {
    req = addToken(req, usuario.token);
    console.log('add token jwt');
  }

  return next(req);
};

// Función para agregar el token al request
function addToken(request: HttpRequest<any>, token: string): HttpRequest<any> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });
}

