namespace MasterMind.Exceptions
{
    public class InvalidMove : Exception
    {
        public InvalidMove()
        {
        }

        public InvalidMove(string? message) : base(message)
        {
        }
    }
}