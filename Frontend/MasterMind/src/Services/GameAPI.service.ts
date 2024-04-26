import { Injectable } from "@angular/core";
import { GenericAPIService } from "./GenericAPI.service";
import { environment } from "../environments/environment.development";
import { IMatch } from "../Interfaces/IMatch";
import { Subject, Subscription } from "rxjs";
import { HttpErrorResponse } from "@angular/common/http";
import { Router } from "@angular/router";
import { IPlayer } from "../Interfaces/IPlayer";


@Injectable({
    providedIn: 'root'
})

export class GameAPIService {
    private gameURL: string;
    userMatch: IMatch | null = null;
    codifedSequence: string[] | null = null;
    invalidResponseInserted = false;

    userMatchSubject: Subject<boolean> = new Subject();
    leaderboardSubject: Subject<IPlayer[]> = new Subject();
    retrivedPlayerSubject: Subject<IPlayer> = new Subject();

    playerRole: string = "";

    private getUserInfo() {
        return {
            Email: JSON.parse(localStorage.getItem("UserEmail") || "{}"),
            Token: JSON.parse(localStorage.getItem("Token") || "{}")
        };
    }

    private removeToken() {
        localStorage.removeItem("Token");
    }

    private navigateToLogin() {
        this.router.navigate(["/"]);
    }

    private formatResponse(response: string[]) {
        let r = 0, b = 0;
        for (let value of response) {
            if (value == 'r') r++;
            else if (value == 'b') b++;
        }

        return `${b}B ${r}R`;
    }

    constructor(private APICaller: GenericAPIService, private router: Router) {
        this.gameURL = environment.GameEndPoint;
    }

    startMatch() {
        const userInfo = this.getUserInfo();
        const startMatchURL = this.gameURL + "/StartMatch";

        this.APICaller.post<IMatch>(
            startMatchURL,
            {
                email: userInfo.Email,
                playerRole: this.playerRole
            },
            undefined,
            true
        ).subscribe(
            {
                next: (match) => {
                    this.userMatch = match;
                    this.router.navigate([("/Match")]);
                    this.userMatchSubject.next(true);
                },
                error: (error: HttpErrorResponse) => {
                    if (error.status == 401) {
                        this.removeToken();
                        this.navigateToLogin();
                        this.userMatchSubject.next(true);
                    }
                    else this.userMatchSubject.next(false);
                }
            }
        );
    }

    restoreMatch() {

        const userInfo = this.getUserInfo();
        const startMatchURL = this.gameURL + "/Match";

        this.APICaller.post<IMatch>(
            startMatchURL,
            {
                playerID: userInfo.Email,
            },
            undefined,
            true
        ).subscribe(
            {
                next: (match) => {
                    this.userMatch = match;
                    this.playerRole = match.playerRole;
                    this.userMatchSubject.next(true);
                },
                error: (error: HttpErrorResponse) => {
                    if (error.status == 401) {
                        this.removeToken();
                        this.navigateToLogin();
                        this.userMatchSubject.next(true);
                    }
                    else this.userMatchSubject.next(false);
                }
            }
        );
    }

    makeMove(sequence?: string[], response?: string[]) {
        const userInfo = this.getUserInfo();

        if (sequence) {
            // Decrypter Move
            const decrpyterMoveURL = this.gameURL + "/Decrypt";


            this.APICaller.post<IMatch>(
                decrpyterMoveURL,
                {
                    playerID: userInfo.Email,
                    sequence: sequence,
                },
                undefined,
                true
            ).subscribe(
                {
                    next: (match) => {
                        this.userMatch = match;
                        this.userMatchSubject.next(true);
                    },
                    error: (error: HttpErrorResponse) => {
                        if (error.status == 401) {
                            this.removeToken();
                            this.navigateToLogin();
                            this.userMatchSubject.next(true);
                        }
                        else this.userMatchSubject.next(false);
                    },
                }
            );
        }
        else if (response) {
            const correctResponse = this.formatResponse(response);
            // Crypter Move
            const crypterMoveURL = this.gameURL + "/Crypt";

            this.APICaller.post<IMatch>(
                crypterMoveURL,
                {
                    playerID: userInfo.Email,
                    response: correctResponse,
                },
                undefined,
                true
            ).subscribe(
                {
                    next: (match) => {
                        if (this.invalidResponseInserted) this.invalidResponseInserted = false;
                        this.userMatch = match;
                        this.userMatchSubject.next(true);
                    },
                    error: (error: HttpErrorResponse) => {
                        if (error.status == 401) {
                            this.removeToken();
                            this.navigateToLogin();
                            this.userMatchSubject.next(true);
                        }
                        else if (error.status == 400) {
                            if (!this.invalidResponseInserted) this.invalidResponseInserted = true;
                            this.userMatchSubject.next(true);
                        }
                        else this.userMatchSubject.next(false);
                    },
                }
            )
        }
        else return;
    }

    sendCodifiedSequence(codifedSequence: string[]) {
        const userInfo = this.getUserInfo();
        const setCodifiedSequenceURL = this.gameURL + "/SetSequence";


        this.APICaller.post<IMatch>(
            setCodifiedSequenceURL,
            {
                playerID: userInfo.Email,
                codifiedSequence: codifedSequence
            },
            undefined,
            true
        ).subscribe(
            {
                next: (match) => {
                    this.userMatch = match;
                    this.userMatchSubject.next(true);
                },
                error: (error: HttpErrorResponse) => {
                    if (error.status == 401) {
                        this.removeToken();
                        this.navigateToLogin();
                        this.userMatchSubject.next(true);
                    }
                    else this.userMatchSubject.next(false);
                }
            }
        )
    }

    retriveLeaderboard() {
        const leaderboardURL = this.gameURL + "/Leaderboard";
        this.APICaller.get<IPlayer[]>(leaderboardURL).subscribe(
            {
                next: (leaderboard) => {
                    for (let player of leaderboard) {
                        player.playerScore = Math.floor(player.playerScore);
                    }
                    this.leaderboardSubject.next(leaderboard);
                },
                error: () => this.router.navigate(["/Error"])
            }
        );
    }

    retrivePlayer() {
        const playerURL = this.gameURL + "/Player";
        const userInfo = this.getUserInfo();
        this.APICaller.get<IPlayer>(playerURL,
            {
                params: { playerID: userInfo.Email }
            }).subscribe(
                {
                    next: (player) => {
                        player.playerScore = Math.floor(player.playerScore);
                        this.retrivedPlayerSubject.next(player);
                    },
                    error: () => {
                        this.router.navigate(["/Error"])
                    }
                }
            );
    }
}