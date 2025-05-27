import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { SessionService } from '../services/session.service';

export const userSessionGuard: CanActivateFn = (route, state) => {
  const sessionService = inject(SessionService);
  const user = sessionService.sessionData;
  console.log(user);
  if (user !== undefined && Object.entries(user).length !== 0) {
    return true;
  }

  return false;
};
