namespace Reimers.Ihe.Communication
{
    using NHapi.Base.Model;
    using NHapi.Base.Parser;

    /// <summary>
    /// Defines the HL7 extension methods.
    /// </summary>
    public static class Hl7Extensions
    {
        /// <summary>
        /// Gets the message control id from the message header.
        /// </summary>
        /// <param name="message">The message to read.</param>
        /// <returns>The message control id as a string.</returns>
        public static string GetMessageControlId(this IMessage message)
        {
            var msh = (ISegment)message.GetStructure("MSH");
            return msh
                    .GetField(10, 0)
                    .ToString();
        }
    }
}