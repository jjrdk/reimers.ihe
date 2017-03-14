namespace Reimers.Ihe
{
    /// <summary>
    /// Defines the container for received HL7 content.
    /// </summary>
    public class Hl7Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hl7Message"/> class.
        /// </summary>
        /// <param name="message">The raw HL7 message.</param>
        /// <param name="sourceAddress">The address the message was received from.</param>
        public Hl7Message(string message, string sourceAddress)
        {
            Message = message;
            SourceAddress = sourceAddress;
        }

        /// <summary>
        /// Gets the address the message was received from.
        /// </summary>
        public string SourceAddress { get; }

        /// <summary>
        /// Gets the raw received HL7 message.
        /// </summary>
        public string Message { get; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (SourceAddress + Message).GetHashCode();
        }
    }
}