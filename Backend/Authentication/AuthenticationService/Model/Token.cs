public class Token
{
    private string _token;
    private DateTime _expireDate;

    public Token(string token)
    {
        _token = token;
        _expireDate = DateTime.Now.AddMinutes(60);
    }

    public bool VerifyToken(string tokenString){
        if(_token == tokenString && _expireDate >= DateTime.Now){
            return true;
        }
        return false;
    }
}