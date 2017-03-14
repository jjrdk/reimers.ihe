namespace Reimers.Ihe.Communication
{
    internal static class Constants
    {
        public static readonly byte[] StartBlock = { 11 };
        public static readonly byte[] EndBlock = { 28, 13 };
        public const string MessageHeaderIdentifier = "MSH";
    }
}