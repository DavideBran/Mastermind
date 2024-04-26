namespace MasterMind.Exceptions
{
    public class NoPlayerRetrived : Exception
    {
        public NoPlayerRetrived()
        {
        }

        public NoPlayerRetrived(string? message) : base(message)
        {
        }
    }
}