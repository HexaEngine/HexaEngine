namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    public struct CBBrush
    {
        public Vector3 Pos;
        public float Radius;

        public float EdgeFadeStart;
        public float EdgeFadeEnd;
        public Vector2 Padding;

        public CBBrush(Vector3 pos, float radius, float edgeFadeStart = 0, float edgeFadeEnd = 1)
        {
            Pos = pos;
            Radius = radius;
            EdgeFadeStart = edgeFadeStart * radius;
            EdgeFadeEnd = edgeFadeEnd * radius;
            Padding = default;
        }

        public readonly float ComputeEdgeFade(float distance)
        {
            return MathUtil.Clamp01((EdgeFadeEnd - distance) / (EdgeFadeEnd - EdgeFadeStart));
        }
    }
}