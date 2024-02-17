namespace HexaEngine.Graphics.Culling
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public struct InstanceData
    {
        public uint Type;
        public Matrix4x4 World;
        public BoundingBox BoundingBox;
        public BoundingSphere BoundingSphere;

        public InstanceData(uint type, Matrix4x4 world, Vector3 min, Vector3 max, Vector3 center, float radius)
        {
            Type = type;
            World = world;
            BoundingBox = new(min, max);
            BoundingSphere = new(center, radius);
        }

        public InstanceData(uint type, Matrix4x4 world, BoundingBox boundingBox, Vector3 center, float radius)
        {
            Type = type;
            World = world;
            BoundingBox = boundingBox;
            BoundingSphere = new(center, radius);
        }

        public InstanceData(uint type, Matrix4x4 world, BoundingBox boundingBox)
        {
            Type = type;
            World = world;
            BoundingBox = boundingBox;
            BoundingSphere = BoundingSphere.CreateFromBoundingBox(boundingBox);
        }

        public InstanceData(uint type, Matrix4x4 world, BoundingBox boundingBox, BoundingSphere boundingSphere)
        {
            Type = type;
            World = world;
            BoundingBox = boundingBox;
            BoundingSphere = boundingSphere;
        }

        public InstanceData(uint type, Matrix4x4 world, BoundingSphere boundingSphere)
        {
            Type = type;
            World = world;
            BoundingBox = new(boundingSphere);
            BoundingSphere = boundingSphere;
        }
    }
}