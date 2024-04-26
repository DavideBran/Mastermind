import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const tokenPresentGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  if (localStorage.getItem("Token")) return true;
  router.navigate(["/"]);
  return false;
};
