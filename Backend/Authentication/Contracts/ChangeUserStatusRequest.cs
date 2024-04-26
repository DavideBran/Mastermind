public record ChangeUserStatusRequest(
    string Email,
    string Token,
    string Status,
    string UserEmail
);