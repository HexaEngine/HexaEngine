namespace HexaEngine.Core.Assets
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MetadataPathNotFoundException : Exception
    {
        public MetadataPathNotFoundException()
        {
        }

        public MetadataPathNotFoundException(string? message) : base(message)
        {
        }

        public MetadataPathNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MetadataPathNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}