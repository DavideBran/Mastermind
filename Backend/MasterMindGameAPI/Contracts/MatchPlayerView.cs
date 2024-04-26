public record MatchView(
    string[]? CodifedSequence,
    string[][] Sequences,
    string [] Responses, 
    string PlayerRole,
    string Host, 
    bool PlayerTurn,
    bool GameEnded
);