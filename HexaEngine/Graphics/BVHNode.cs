namespace HexaEngine.Graphics
{
    using Hexa.NET.Mathematics;

    public struct BVHNode<T>
    {
        public BoundingBox Box;
        public T Value;
        public int ParentIndex;
        public int Child1;
        public int Child2;
        public bool IsLeaf;
    }
}