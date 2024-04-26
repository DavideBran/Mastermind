import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { tokenPresentGuard } from './token-present.guard';

describe('tokenPresentGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => tokenPresentGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
