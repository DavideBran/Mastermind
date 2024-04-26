import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { MatchComponent } from './match/match.component';
import { tokenPresentGuard } from './token-present.guard';
import { ErrorComponent } from './error/error.component';
import { MastermindHomeComponent } from './mastermind-home/mastermind-home.component';
import { tokenNotPresentGuard } from './token-not-present.guard';

export const routes: Routes = [
    {
        path: '',
        component: LoginComponent, 
        pathMatch: 'full',
        canActivate: [tokenNotPresentGuard]
    },
    {
        path: 'Mastermind',
        component: MastermindHomeComponent,
        canActivate: [tokenPresentGuard]
    },
    {
        path: 'Match',
        component: MatchComponent,
        canActivate: [tokenPresentGuard]
    },
    {
        path: 'Error',
        component: ErrorComponent
    },
    {
        path: '**', redirectTo: ''
    }
];
