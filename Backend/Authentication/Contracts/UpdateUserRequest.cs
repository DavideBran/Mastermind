public record UpdateUserRequest(
    string UserToUpdateEmail, 
    string AdminEmail,
    string AdminToken, 
    // Attribute To update 
    string Name = "undefined", 
    string Email = "undefined"
);  