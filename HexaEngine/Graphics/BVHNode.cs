namespace HexaEngine.Graphics
{
    using Hexa.NET.Mathematics;

    public struct BVHNode
    {
        public BoundingBox Box;
        public int ObjectIndex;
        public int ParentIndex;
        public int Child1;
        public int Child2;
        public bool IsLeaf;
    }
}