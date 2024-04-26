import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { LoginService } from '../../Services/login.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  userPassword = '';
  userEmail = '';
  invalidCredential = false;

  constructor(private userAPI: LoginService, private router: Router) { }

  showError() {
    this.invalidCredential = true;
  }

  async onSumbit() {

    // Call the UserAPIService for logging in 
    const response$ = await this.userAPI.login(this.userEmail, this.userPassword);

    const responseSubscription: Subscription = response$.subscribe(
      {
        next: (userToken) => {
          if (userToken.toLowerCase() == "error") this.showError();
          else {
            window.localStorage.setItem("UserEmail", JSON.stringify(this.userEmail));
            window.localStorage.setItem("Token", JSON.stringify(userToken));
            // redirect
            this.router.navigate(["/Mastermind"]);
          }
        },
        complete: () => responseSubscription.unsubscribe()
      }
    );
  }
}
