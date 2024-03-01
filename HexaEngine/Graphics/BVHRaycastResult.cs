namespace HexaEngine.Graphics
{
    public record struct BVHRaycastResult(int child, float depth)
    {
        public int Child = child;
        public float Depth = depth;
    }
}