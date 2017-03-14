namespace Reimers.Ihe.Communication
{
    using System.Threading.Tasks;    /// <summary>
    /// Defines the public interface for middleware for handling HL7 messages.
    /// </summary>
    public interface IHl7MessageMiddleware
    {
        /// <summary>
        /// Handles the passed <see cref="Hl7Message"/> message.
        /// </summary>
        /// <param name="message">The <see cref="Hl7Message"/> to handle.</param>
        /// <returns>An HL7 response as a <see cref="string"/>.</returns>
        Task<string> Handle(Hl7Message message);
    }
}