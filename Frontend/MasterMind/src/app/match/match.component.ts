import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { GameAPIService } from '../../Services/GameAPI.service';
import { Router } from '@angular/router';
import { IMatch } from '../../Interfaces/IMatch';
import { Subscription } from 'rxjs';
import { PageInformationService } from '../../Services/PageInformation.service';

@Component({
  selector: 'app-match',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './match.component.html',
  styleUrl: './match.component.css'
})
export class MatchComponent implements OnInit, OnDestroy {

  darkMode: boolean = false;

  userMatchSubscription: Subscription | null = null;
  differentColor = false;


  sequences: string[][] = new Array(9);
  responses: string[][] | null = null;
  playerRole: string | null = null;
  turns: number = 0;
  winnedMatch: boolean = false;
  matchEndMessage: string = "";


  avaibleColors = ["blue", "orange", "gray", "purple", "white", "pink", "red", "green"];
  allSequences = Array.from(new Array(9).keys()).map((index) => index == 0);
  allColors = Array.from(new Array(8).keys()).map(() => false);

  matchInfo: IMatch | null = null;

  lastColor: string = "";
  userResponse: string = "";
  invalidSequence: boolean = false;
  invalidResponse: boolean = false;
  won: boolean = false;
  codifedSequenceSubmitted: boolean = false;
  codifedSequence: string[] = new Array(4);

  private showWinBedge() {
    this.checkWin();
    this.winnedMatch = true;
  }

  private matchAlreadyStarted() {
    for (let i = 0; i < this.codifedSequence.length; i++) {
      if (this.codifedSequence[i].toLowerCase() != "white") return true;
    }
    return false;
  }

  private setupCodifiedSequence() {
    // checking if the match is an already started match (in other word if the sequence has been already inserted in the past) 
    if (this.matchAlreadyStarted()) {
      this.codifedSequenceSubmitted = true;
      return;
    }

    for (let i = 0; i < this.codifedSequence.length; i++) {
      this.codifedSequence[i] = "";
    }
  }

  private setupMatch() {
    this.matchInfo = this.gameService.userMatch;
    this.playerRole = this.gameService.playerRole;
    if (this.matchInfo?.codifedSequence != null) {
      this.codifedSequence = this.matchInfo.codifedSequence;
      this.setupCodifiedSequence();
    }
    this.setupResponses();
    this.setupSequences();
    this.findCurrentTurn();
  }

  private navigateToError() {
    this.router.navigate(["/Error"]);
  }

  private setupResponses() {
    if (this.matchInfo == null) return;
    else {
      this.formatAllResponses();
    }
  }

  private setupSequences() {
    if (this.matchInfo == null) return;
    else {
      this.formatAllSequences();
    }
  }

  private validateSequence(color: string, sequence: string[]) {
    let count = 0;
    for (let i = 0; i < sequence.length; i++) {
      if (color == sequence[i]) count++;
    }
    if (count >= 1) this.differentColor = true;
    else this.differentColor = false;
    return !(count >= 1);
  }

  private formatAllResponses() {
    if (this.matchInfo == null) return;
    this.responses = new Array(this.matchInfo.responses.length);
    for (let i = 0; i < this.matchInfo.responses.length; i++) {
      if (this.matchInfo.responses[i] == null) this.responses[i] = ["", "", "", ""];
      else this.responses[i] = this.formatResponse(this.matchInfo.responses[i]);
    }
  }

  private formatResponse(response: string) {
    const splicedResponse = response.split(" ");
    const correctColor = Number(splicedResponse[1][0]);
    const corrcetColorAndPosition = Number(splicedResponse[0][0]);


    let formattedResponse = new Array(4);
    for (let i = 0; i < correctColor; i++) {
      formattedResponse[i] = "r";
    }
    for (let i = correctColor; i < correctColor + corrcetColorAndPosition; i++) {
      formattedResponse[i] = "b";
    }

    return formattedResponse;
  }

  private formatSequence(sequence: string[]) {
    let formattedSequence = new Array(sequence.length);
    for (let j = 0; j < sequence.length; j++) {
      formattedSequence[j] = sequence[j].toLowerCase();
    }
    return formattedSequence;
  }

  private formatAllSequences() {
    if (this.matchInfo == null) return;

    this.sequences = new Array(this.matchInfo.sequences.length);
    for (let i = 0; i < this.matchInfo.sequences.length; i++) {
      if (this.matchInfo.sequences[i] == null) this.sequences[i] = ["", "", "", ""];
      else {
        this.sequences[i] = this.formatSequence(this.matchInfo.sequences[i]);
      }
    }
  }

  private findTurn() {
    let turn = 0;
    for (let i = 0; i < this.sequences.length; i++) {
      if (this.sequences[i][0] == "") break;
      else turn++;
    }
    return turn;
  }

  private findCurrentTurn() {
    this.turns = 0;
    if (this.playerRole?.toLocaleLowerCase() == "crypter") this.turns = this.findTurn() - 1;
    else this.turns = this.findTurn();

    this.allSequences = this.allSequences.map(() => false);
    this.allSequences[this.turns] = true;
  }

  private checkCrypterWin() {
    if (this.responses == null) return "";
    if (this.turns > 8) this.turns = 8;
    if (this.turns >= 5) {
      this.won = true;
      return 'You Won!';
    }
    let message = 'You Lose!';
    this.won = false;
    for (let value of this.responses[this.turns]) {
      // if i find a value that is not b it means that the crypter player won the match (the computer doasn't found the codify)
      if (value != 'b') {
        message = 'You Won!';
        this.won = true;
        return message;
      }
    }
    return message;
  }

  private checkDecrypterWin() {
    if (this.responses == null) return "";
    let message = 'You Won!';
    this.won = true;
    for (let value of this.responses[this.turns - 1]) {
      // if the value ar all b it means that the decrypter found the sequence
      if (value != 'b') {
        message = 'You Lose!';
        this.won = false;
        return message;
      }
    }
    return message;
  }

  private checkWin() {
    this.matchEndMessage = this.playerRole?.toLocaleLowerCase() == 'crypter' ? this.checkCrypterWin() : this.checkDecrypterWin();
  }

  changeMode() {
    this.pageInfo.updateMode();
    this.darkMode = this.pageInfo.darkMode;
  }

  setResponse(responsesIndex: number, responsePosition: number) {
    if (responsesIndex != this.turns) return;
    if (this.validateCodifiedSequence() && !this.codifedSequenceSubmitted) return;
    this.differentColor = false;
    if (this.responses == null || this.playerRole?.toLocaleLowerCase() != "crypter") return;
    if (this.responses[responsesIndex][responsePosition] == "") this.responses[responsesIndex][responsePosition] = "r";
    else if (this.responses[responsesIndex][responsePosition] == "r") this.responses[responsesIndex][responsePosition] = "b";
    else if (this.responses[responsesIndex][responsePosition] == "b") this.responses[responsesIndex][responsePosition] = "";
  }

  insertIntoCodify(index: number) {
    if (!this.validateCodifiedSequence()) return;
    this.codifedSequence[index] = this.lastColor;
  }

  selectColor(index: number) {
    this.allColors = this.allColors.map(() => false);
    this.allColors[index] = true;
    this.lastColor = this.avaibleColors[index];
  }

  validateCodifiedSequence() {
    let color = this.lastColor;
    this.differentColor = false;
    for (let i = 0; i < this.codifedSequence.length; i++) {
      if (!this.codifedSequence[i]) return true;
      if (color == this.codifedSequence[i]) {
        this.differentColor = true;
        return false;
      }
    }
    return true;
  }

  countValidColor() {
    let count = 0;
    for (let color of this.sequences[this.turns]) {
      if (color) count++;
    }

    return count == 4;
  }

  validateParent(sequenceColor: HTMLElement) {
    if (sequenceColor.parentElement?.parentElement?.classList.contains("selected")) return true;
    return false;
  }

  insertColorSelected(sequenceColor: HTMLElement, symbolIndex: number) {
    if (this.playerRole?.toLocaleLowerCase() != 'decrypter') return;
    if (this.lastColor == "" || !this.validateParent(sequenceColor)) return;

    if (!this.validateSequence(this.lastColor, this.sequences[this.turns])) this.invalidSequence = true;
    else {
      this.sequences[this.turns][symbolIndex] = this.lastColor;
    }
  }

  validateColorsSelected() {
    for (let color of this.sequences[this.turns]) {
      if (!color) return false;
    }
    return true;
  }

  sendMove() {
    if (this.playerRole?.toLowerCase() == "decrypter" && this.countValidColor()) {
      this.gameService.makeMove(this.sequences[this.turns])
    }
    else {
      if (this.responses == null) return;
      this.gameService.makeMove(undefined, this.responses[this.turns]);
    }
  }

  sendCodify() {
    if (this.playerRole?.toLowerCase() != "crypter") return;
    this.gameService.sendCodifiedSequence(this.codifedSequence);
  }

  navigateToHome() {
    this.router.navigate(["/Mastermind"]);
  }

  invalidResponseInserted() {
    this.invalidResponse = !this.invalidResponse;
  }

  constructor(private gameService: GameAPIService, private router: Router, private pageInfo: PageInformationService) {
  }

  ngOnInit() {
    this.darkMode = this.pageInfo.darkMode;
    this.userMatchSubscription = this.gameService.userMatchSubject.subscribe(
      {
        next: (matchRetrived) => {
          if (matchRetrived) {
            if (this.gameService.invalidResponseInserted) {
              this.invalidResponseInserted();
            }
            else {
              this.matchInfo = this.gameService.userMatch;
              this.playerRole = this.gameService.playerRole;
              this.setupMatch();
              if (this.matchInfo?.gameEnded) {
                this.showWinBedge();
                return;
              }
            }
          }
          else {
            this.navigateToError();
          }
        }
      }
    );

    if (!this.gameService.userMatch) {
      this.gameService.restoreMatch();
    }
    else this.setupMatch();
  }

  ngOnDestroy() {
    if (this.userMatchSubscription) this.userMatchSubscription.unsubscribe();
  }
}


