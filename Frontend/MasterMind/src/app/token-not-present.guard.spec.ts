import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { tokenNotPresentGuard } from './token-not-present.guard';

describe('tokenNotPresentGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => tokenNotPresentGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
