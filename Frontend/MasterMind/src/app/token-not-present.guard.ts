import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const tokenNotPresentGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);

  if(localStorage.getItem("Token")) {
    router.navigate(["/Mastermind"]);
    return false; 
  }

  return true;
};
