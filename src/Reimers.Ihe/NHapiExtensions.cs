namespace Reimers.Ihe
{
    using System;
    using System.Globalization;
    using NHapi.Base.Parser;

    public static class NHapiExtensions
    {
        public static string GetMessageControlId(this PipeParser parser, string message)
        {
            var ackId = parser.GetAckID(message);
            if (ackId != null)
            {
                return ackId;
            }

            var startIndex =
                message.IndexOf(Constants.MessageHeaderIdentifier);
            var num = message.IndexOf('\r', startIndex + 1);
            var composite = message.Substring(
                startIndex,
                num - startIndex);
            var parts = composite.Split('|');
            ackId = parts[9];

            return ackId;
        }
    }
}