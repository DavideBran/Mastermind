export interface IMatch{
    codifedSequence: string[],
    sequences: string[][],
    responses: string [], 
    host: string, 
    playerRole: string,
    playerTurn: boolean,
    gameEnded: boolean
}