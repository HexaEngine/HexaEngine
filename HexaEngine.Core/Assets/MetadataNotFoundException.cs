namespace HexaEngine.Core.Assets
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MetadataNotFoundException : Exception
    {
        public MetadataNotFoundException()
        {
        }

        public MetadataNotFoundException(string? message) : base(message)
        {
        }

        public MetadataNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}