namespace HexaEngine.Core.Graphics
{
    public struct GBufferDescription
    {
        public int Width;
        public int Height;
        public int Count;
        public Format[] Formats;

        public GBufferDescription(int width, int height, int count, params Format[] formats)
        {
            Width = width;
            Height = height;
            Count = count;
            Formats = formats;
        }
    }
}