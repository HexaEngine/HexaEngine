namespace HexaEngine.UI.Graphics.Text
{
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

        public BufferCurve(float x0, float y0, float x1, float y1, float x2, float y2)
        {
            V0 = new(x0, y0);
            V1 = new(x1, y1);
            V2 = new(x2, y2);
        }
    }
}