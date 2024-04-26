import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { GameAPIService } from '../../Services/GameAPI.service';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { IPlayer } from '../../Interfaces/IPlayer';
import { InstructionComponent } from "../instruction/instruction.component";
import { PageInformationService } from '../../Services/PageInformation.service';

@Component({
    selector: 'app-mastermind-home',
    standalone: true,
    templateUrl: './mastermind-home.component.html',
    styleUrl: './mastermind-home.component.css',
    imports: [CommonModule, InstructionComponent]
})
export class MastermindHomeComponent implements OnInit, OnDestroy {
  darkMode : boolean = false;
  playerFriends = 0;
  playerPlayTime = 0;
  playerID = JSON.parse(localStorage.getItem("UserEmail") || "{}");
  player: IPlayer | null = null;
  leaderboard: IPlayer[] | null = null;


  error = false;
  noMatchRetrived = false;

  userMatchSubscription: Subscription | null = null;
  playerRetrivedSubscription: Subscription | null = null;
  leaderboardRetrivedSubscription: Subscription | null = null;


  private showError() {
    this.error = true;
  }

  constructor(private gameService: GameAPIService, private router: Router, private pageInfo : PageInformationService) { }

  logout(){
    localStorage.removeItem("Token"); 
    this.router.navigate(["/"]);
  }

  matchRequest(event: Event) {
    const target: HTMLElement = event.target as HTMLElement;
    if (target.classList.contains("startMatch")) {
      this.gameService.playerRole = target.id;

      this.gameService.startMatch();
      this.gameService.userMatchSubject.subscribe(
        {
          next: (matchStarted) => {
            if (!matchStarted) this.showError();
          },
        }
      );
    }
    else {
      this.router.navigate(["/Match"])
    }
  }

  changeMode(){
    this.pageInfo.updateMode();
    this.darkMode = this.pageInfo.darkMode;
  }

  ngOnInit(): void {
    this.darkMode = this.pageInfo.darkMode;
    
    this.userMatchSubscription = this.gameService.userMatchSubject.subscribe(
      {
        next: (matchRetrived) => {
          if (!matchRetrived) this.noMatchRetrived = true;
        }
      }
    );

    this.playerRetrivedSubscription = this.gameService.retrivedPlayerSubject.subscribe(
      {
        next: (playerRetrived) => {
          if (!this.player) this.player = playerRetrived;
          this.playerPlayTime = this.player.crypterTimes + this.player.decrypterTimes;
        }
      }
    );

    this.leaderboardRetrivedSubscription = this.gameService.leaderboardSubject.subscribe(
      {
        next: (leaderboardRetrived) => {
          this.leaderboard = leaderboardRetrived}
      }
    );

    this.gameService.restoreMatch();
    if (!this.player) this.gameService.retrivePlayer();
    if (!this.leaderboard) this.gameService.retriveLeaderboard();
  }

  ngOnDestroy() {
    if (this.userMatchSubscription) this.userMatchSubscription.unsubscribe();
    if(this.leaderboardRetrivedSubscription) this.leaderboardRetrivedSubscription.unsubscribe();
    if(this.playerRetrivedSubscription) this.playerRetrivedSubscription.unsubscribe();
  }
}
