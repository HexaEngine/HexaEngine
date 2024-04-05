namespace HexaEngine.Graphics
{
    public record struct OctreeRaycastResult(int child, float depth)
    {
        public int Child = child;
        public float Depth = depth;
    }
}