using System.Net;
using System.Net.Mail;

public class UserManager
{
    private UserDataManager _userData;

    private string GenerateRandomNewUserPassowrd()
    {
        Random random = new();
        string[] character = { "/", "&", "%", "(", "a", "b", "c", "d", "j" };
        string password = "";

        for (byte i = 0; i < character.Length; i++)
        {
            password += character[random.Next(0, character.Length)];
        }

        return password;

    }

    private void SendPasswordTo(string name, string email, string password)
    {
        var senderPsw = "AuthentService";

        SmtpClient client = new("smtp-mail.outlook.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential("authenticationserv@outlook.com", senderPsw)
        };

        client.Send(
            new MailMessage("authenticationserv@outlook.com", email, "Authentication Service Password", $"{name} your passowrd is {password}")
        );
    }

    public UserManager()
    {
        _userData = new UserDataManager();
    }

    public User Get(string email)
    {
        return _userData.Get(email);
    }

    public string GetNonce(string email)
    {
        try
        {
            User retrivedUser = Get(email);
            if (retrivedUser.State == User.Status.BLOCK)
            {
                int randomValue = new Random(DateTime.Now.Millisecond).Next();
                return $"87456765{randomValue}";
            }
            return retrivedUser.GenerateChallenge();
        }
        catch (UserNotRegistered)
        {
            return $"{new Random(DateTime.Now.Millisecond + 100).Next()}{new Random(DateTime.Now.Millisecond).Next()}";
        }
    }

    public string VerifyUser(User user, string codifiedPassword)
    {
        if (!user.VerifyPassword(codifiedPassword)) throw new UserNotAuthenticate();
        _userData.Save();
        return user.GenerateToken();
    }

    public void VerifyToken(string email, string token)
    {
        User userToVerify = Get(email);
        if (userToVerify.State == User.Status.BLOCK || !userToVerify.VerifyToken(token)) throw new UnauthorizedAccessException();
    }

    public void RegisterUser(string email, string name)
    {
        //check if the User Already Exist
        try
        {
            _userData.Get(email);
            throw new AlreadyRegistered();
        }
        catch (UserNotRegistered)
        {
            string password = GenerateRandomNewUserPassowrd();
            var user = new User(
                name,
                email,
                password,
                User.Roles.USER
            );
            _userData.Insert(user);
            SendPasswordTo(user.Name, user.Email, password);
        }
    }

    public void UpdateUser(string userToUpdateEmail, string name, string email)
    {
        User userToUpdate = _userData.Get(userToUpdateEmail);
        if (name != "undefined") userToUpdate.UpdateName(name);
        if (email != "undefined") userToUpdate.UpdateEmail(email);
        _userData.Save();
    }

    public List<User> GetUsersView()
    {
        return (List<User>)_userData.GetAll();
    }
}