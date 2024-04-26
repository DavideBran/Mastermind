import { Injectable } from '@angular/core';
import { GenericAPIService } from './GenericAPI.service';
import { Subject, Subscription } from 'rxjs';
import { environment } from '../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class LoginService {

  private loginURL = 'http://localhost:5049/Login';


  private async hashSHA256(password: string) {
    const buffer = new TextEncoder().encode(password);
    const hashBuffer = await crypto.subtle.digest('SHA-256', buffer);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashHex = hashArray.map(byte => byte.toString(16).padStart(2, '0')).join('');
    return hashHex;
  }

  private async getLoginRequest(email: string, password: string, nonce: string) {
    //for security reason the password hash is disabled
    // password = await this.hashSHA256(password);

    // const codifiedPassword = await this.hashSHA256(password + nonce);
    const codifiedPassword = `${password}${nonce}`;

    return this.APICaller.post<string>(
      this.loginURL,
      {
        Email: email,
        codifiedPassword: codifiedPassword
      }
    );
  }

  constructor(private APICaller: GenericAPIService) {
    this.loginURL = environment.GameEndPoint + "/Login";
  }

  async login(email: string, password: string) {
    const loginSubject: Subject<string> = new Subject<string>();


    const getNonce$ = this.APICaller.get<string>(
      this.loginURL,
      {
        params: { email: email },
      }
    );

    // Starting the Login request 
    const nonceSubscription: Subscription = getNonce$.subscribe(
      {
        next: async (nonce) => {
          // generating the login request, with the nonce
          const login$ = await this.getLoginRequest(email, password, nonce);

          const loginSubscription: Subscription = login$.subscribe(
            {
              next: (userToken) => loginSubject.next(userToken),
              error: () => loginSubject.next("error"),
              complete: () => loginSubscription.unsubscribe()
            });
        },
        complete: () => nonceSubscription.unsubscribe()
      }
    );
    return loginSubject.asObservable();
  }

}
