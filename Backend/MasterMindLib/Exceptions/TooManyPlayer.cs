namespace MasterMind.Exceptions
{
    public class TooManyPlayer : Exception
    {
        public TooManyPlayer()
        {
        }

        public TooManyPlayer(string? message) : base(message)
        {
        }
    }

}