public record CrypterSetSequenceRequest(
    string PlayerID,
    string[] CodifiedSequence,
    string Token
);