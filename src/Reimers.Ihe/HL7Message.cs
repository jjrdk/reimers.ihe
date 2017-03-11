namespace Reimers.Ihe
{
    public class Hl7Message
    {
        public Hl7Message(string message, string sourceAddress)
        {
            Message = message;
            SourceAddress = sourceAddress;
        }

        public string SourceAddress { get; }

        public string Message { get; }

        public override int GetHashCode()
        {
            return (SourceAddress + Message).GetHashCode();
        }
    }
}