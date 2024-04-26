namespace MasterMind.Exceptions
{
    public class InvalidResponse : Exception
    {
        public InvalidResponse()
        {
        }

        public InvalidResponse(string? message) : base(message)
        {
        }
    }
}