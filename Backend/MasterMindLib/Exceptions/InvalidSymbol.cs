namespace MasterMind.Exceptions
{
    public class InvalidSymbol : Exception
    {
        public InvalidSymbol()
        {
        }

        public InvalidSymbol(string? message) : base(message)
        {
        }
    }
}