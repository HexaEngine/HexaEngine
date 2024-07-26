namespace HexaEngine.Graphics.Culling
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    public struct GPUInstance
    {
        public uint Type;
        public Matrix4x4 World;
        public BoundingSphere BoundingSphere;

        public GPUInstance(uint type, Matrix4x4 world, Vector3 center, float radius)
        {
            Type = type;
            World = world;
            BoundingSphere = new(center, radius);
        }

        public GPUInstance(uint type, Matrix4x4 world, BoundingBox boundingBox)
        {
            Type = type;
            World = world;
            BoundingSphere = BoundingSphere.CreateFromBoundingBox(boundingBox);
        }

        public GPUInstance(uint type, Matrix4x4 world, BoundingSphere boundingSphere)
        {
            Type = type;
            World = world;
            BoundingSphere = boundingSphere;
        }
    }
}