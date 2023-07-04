namespace HexaEngine.Core.Lights
{
    using System.Numerics;

    public struct ClusterSizes
    {
        public Vector2 Size;
        public Vector2 padd;

        public ClusterSizes(float width, float height)
        {
            Size = new(width, height);
            padd = default;
        }
    }
}