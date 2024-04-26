using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

public class User
{

    public enum Roles { ADMIN, USER }
    public enum Status { BLOCK, ACTIVE }
    [JsonProperty]
    private Guid _id;
    private string _name;
    private string _email;
    [JsonProperty]
    private string _password;
    private Token _token;
    private string _challenge;
    private Status _status;
    [JsonProperty]
    private int _logTime;

    [JsonProperty]
    private Roles _role = new();

    private string HashPasswordSHA256(string password)
    {
        // Converting the string into a Byte Array
        byte[] passwordInByte = Encoding.UTF8.GetBytes(password);
        // Hashing the string converted byte per byte (i'm using the SHA256 class)
        byte[] hashedPasswordInByte = SHA256.HashData(passwordInByte);

        // Creating the string with StringBuilder (more efficent on creating a string with concatenation)
        StringBuilder hashedPassword = new();
        foreach (var charByte in hashedPasswordInByte)
        {
            // Passing x2 as paramater on ToString method to convert the string into Exadecimal string (2 is for the length of the string, max 2)
            hashedPassword.Append(charByte.ToString("x2"));
        }

        return hashedPassword.ToString();
    }

    public string Name { get => _name; }
    public string Email { get => _email; }
    public Status State { get => _status; }
    public int LogTime { get => _logTime; }
    public Guid ID { get => _id; }
    public Roles GetRole()
    {
        return _role;
    }

    [JsonConstructor]
    public User(string Name, string Email, string Paswword, Roles role, Stack<string> log, int logTime, Status state)
    {
        _name = Name;
        _email = Email;
        _password = Paswword;
        _role = role;
        _logTime = logTime;
        _status = state;
    }

    public User(string name, string email, string password, Roles MAINrole)
    {
        _status = Status.ACTIVE;
        _role = MAINrole;
        _id = Guid.NewGuid();
        _name = name;
        _email = email;
        _password = HashPasswordSHA256(password);
        _logTime = 0;
    }

    public string GenerateToken()
    {
        Random random = new Random();
        string tokenString = _id.ToString() + _email.GetHashCode().ToString() + random.Next().GetHashCode().ToString();
        _token = new Token(tokenString);
        return tokenString;

    }

    public string GenerateChallenge()
    {
        if (_status != Status.ACTIVE) throw new UnauthorizedAccessException();
        Random random = new Random();
        _challenge = $"{_email.GetHashCode()}{random.Next().GetHashCode()}";
        return _challenge;
    }

    public bool VerifyToken(string tokenString)
    {
        if (_status != Status.ACTIVE) return false;
        return _token.VerifyToken(tokenString);
    }

    public bool VerifyPassword(string challengedPassword)
    {

        // i'm expeting that the password that arrive is built in this way: hash((hashedPassword + Nonce))
        if (_challenge == "") throw new UserNotAuthenticate();
        //for security Reason the password hash is disabled 
        // string expectedPassword = HashPasswordSHA256(_password + _challenge);

        // removing the challenge i sended, (if the challenge is different the hash will be different, infact the wrong challenge inserted will be not removed and the hash will be different);
        string hashedPsw = HashPasswordSHA256(challengedPassword.Replace(_challenge, ""));
        challengedPassword = hashedPsw;
        // need to confront the hashedPsw that arrived with my hashed psw (after i removed the aspected challenge)
        // string expectedPassword = _password + _challenge;
        string expectedPassword = _password;

        _challenge = "";
        if (expectedPassword == challengedPassword)
        {
            _logTime++;
            return true;
        }
        return false;
    }

    public bool ChangeRole(Roles role, User applicant)
    {
        if (applicant.GetRole() == Roles.ADMIN) return false;
        _role = role;
        return true;
    }

    public bool ChangeStatus(string status, User applicant)
    {
        if (applicant.GetRole() != Roles.ADMIN) throw new UnauthorizedAccessException();
        if (status.ToUpper() == "BLOCK")
        {
            _status = Status.BLOCK;
            return true;
        }
        else if (status.ToUpper() == "ACTIVE")
        {
            _status = Status.ACTIVE;
            return true;
        }
        return false;
    }

    public void UpdateEmail(string email)
    {
        _email = email;
    }

    public void UpdateName(string name)
    {
        _name = name;
    }

    public string GetInfo()
    {
        return $"Name: {Name} || Email: {Email} || ID: {ID}";
    }
}