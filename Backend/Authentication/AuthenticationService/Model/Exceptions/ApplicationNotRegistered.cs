public class ApplicationNotRegistered : Exception
{
    public ApplicationNotRegistered() : base() { }
    public ApplicationNotRegistered(string message) : base(message) { }

}