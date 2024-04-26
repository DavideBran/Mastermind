public class UserNotRegisteredIntoApp : Exception
{

    public UserNotRegisteredIntoApp() : base() { }
    public UserNotRegisteredIntoApp(string message) : base(message) { }
}