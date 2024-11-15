namespace HexaEngine.Editor.UI
{
    using System;
    using System.Numerics;

    public abstract class TitleBarElement : ITitleBarElement
    {
        protected static uint ABGRLerp(uint colorA, uint colorB, float s)
        {
            var colA = ABGRU32ToRGBAV4(colorA);
            var colB = ABGRU32ToRGBAV4(colorB);
            var lerp = Vector4.Lerp(colA, colB, s);
            return RGBAV4ToABGRU32(lerp);
        }

        protected static uint RGBALerp(Vector4 colorA, Vector4 colorB, float s)
        {
            return RGBAV4ToABGRU32(Vector4.Lerp(colorA, colorB, s));
        }

        protected static Vector4 ABGRU32ToRGBAV4(uint color)
        {
            byte a = (byte)(color >> 24 & 0xFF);
            byte b = (byte)(color >> 16 & 0xFF);
            byte g = (byte)(color >> 8 & 0xFF);
            byte r = (byte)(color & 0xFF);
            return new Vector4(r / (float)byte.MaxValue, g / (float)byte.MaxValue, b / (float)byte.MaxValue, a / (float)byte.MaxValue);
        }

        protected static uint RGBAV4ToABGRU32(Vector4 color)
        {
            byte r = (byte)(Math.Clamp(color.X, 0, 1) * byte.MaxValue);
            byte g = (byte)(Math.Clamp(color.Y, 0, 1) * byte.MaxValue);
            byte b = (byte)(Math.Clamp(color.Z, 0, 1) * byte.MaxValue);
            byte a = (byte)(Math.Clamp(color.W, 0, 1) * byte.MaxValue);
            return (uint)(a << 24 | b << 16 | g << 8 | r);
        }

        public abstract Vector2 Size { get; }

        public abstract string Label { get; }

        public abstract bool IsVisible { get; }

        public abstract void Draw(TitleBarContext context);
    }
}