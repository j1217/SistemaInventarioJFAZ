import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  // Redirige al login si la sesión no contiene un token vigente.
  return auth.isAuthenticated() ? true : inject(Router).createUrlTree(['/login']);
};
