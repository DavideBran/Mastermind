public class AlreadyRegistered : Exception
{
    public AlreadyRegistered() : base() { }
    public AlreadyRegistered(string message) : base(message) { }

}