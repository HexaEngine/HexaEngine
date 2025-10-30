namespace HexaEngine.UI.Graphics.Text
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    public struct BufferCurve
    {
        public Vector2 V0;
        public Vector2 V1;
        public Vector2 V2;

        public BufferCurve(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        public BufferCurve(Vector2D v0, Vector2D v1, Vector2D v2)
        {
            V0 = new((float)v0.X, (float)v0.Y);
            V1 = new((float)v1.X, (float)v1.Y);
            V2 = new((float)v2.X, (float)v2.Y);
        }

        public BufferCurve(float x0, float y0, float x1, float y1, float x2, float y2)
        {
            V0 = new(x0, y0);
            V1 = new(x1, y1);
            V2 = new(x2, y2);
        }
    }
}