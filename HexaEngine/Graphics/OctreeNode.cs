namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System.Runtime.CompilerServices;

    public unsafe struct OctreeNode
    {
        public BoundingBox Bounds;
        public UnsafeList<OctreeObject> Objects;
        public int ParentIndex;
        public ChildrenArray Children;

        [InlineArray(8)]
        public struct ChildrenArray
        {
            public int Element0;
        }

        public OctreeNode(BoundingBox bounds)
        {
            Bounds = bounds;
            Objects = [];
        }

        public readonly bool IsLeafNode()
        {
            return Children[0] == 0;
        }
    }
}