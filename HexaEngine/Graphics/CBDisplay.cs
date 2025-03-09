namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;

    public struct CBDisplay
    {
        public float SDRWhitePoint;
        public float DisplayMaxNits;
        public ColorSpace ColorSpace;
        public float Padding;

        public CBDisplay(float sdrWhitePoint, float displayMaxNits, ColorSpace colorSpace)
        {
            SDRWhitePoint = sdrWhitePoint;
            DisplayMaxNits = displayMaxNits;
            ColorSpace = colorSpace;
        }
    }
}