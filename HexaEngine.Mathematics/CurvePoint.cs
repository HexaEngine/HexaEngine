namespace HexaEngine.Mathematics
{
    using System.Numerics;

    public struct CurvePoint
    {
        public float X;
        public float Y;
        public CurvePointType Type;

        public CurvePoint(Vector2 position, CurvePointType type)
        {
            X = position.X;
            Y = position.Y;
            Type = type;
        }

        public CurvePoint(float x, float y, CurvePointType type)
        {
            X = x;
            Y = y;
            Type = type;
        }

        public Vector2 Pos
        {
            readonly get => new(X, Y);
            set { X = value.X; Y = value.Y; }
        }
    }
}