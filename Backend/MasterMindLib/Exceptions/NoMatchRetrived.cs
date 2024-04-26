namespace MasterMind.Exceptions
{
    public class NoMatchRetrived : Exception
    {
        public NoMatchRetrived() { }

        public NoMatchRetrived(string? message) : base(message) { }
    }
}