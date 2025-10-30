namespace HexaEngine.UI
{
    using System.ComponentModel;

    [TypeConverter(typeof(DeferrableContentConverter))]
    public class DeferrableContent
    {
        private readonly Stream _contentStream;

        // Internal constructor
        internal DeferrableContent(Stream contentStream)
        {
            _contentStream = contentStream;
        }

        // Method to access the content stream
        internal Stream GetContentStream()
        {
            return _contentStream;
        }

        // Dispose method to release the stream
        internal void Dispose()
        {
            _contentStream?.Dispose();
        }
    }
}