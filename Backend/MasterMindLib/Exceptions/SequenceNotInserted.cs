namespace MasterMind.Exceptions
{
    public class SequenceNotInserted : Exception
    {
        public SequenceNotInserted() { }

        public SequenceNotInserted(string? message) : base(message) { }
    }
}