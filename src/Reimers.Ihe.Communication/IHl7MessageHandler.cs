namespace Reimers.Ihe.Communication
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the public interface for handling HL7 messages.
    /// </summary>
    public interface IHl7MessageHandler
    {
        /// <summary>
        /// Gets the structure names that are handled by this handler.
        /// </summary>
        IEnumerable<string> Handles { get; }

        /// <summary>
        /// Handles the passed message.
        /// </summary>
        /// <param name="message">The <see cref="Hl7Message"/> to handle.</param>
        /// <returns>An HL7 response as a <see cref="string"/>.</returns>
        Task<string> Handle(Hl7Message message);
    }
}