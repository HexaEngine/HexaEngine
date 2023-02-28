namespace HexaEngine.Core.Instances
{
    using System.Numerics;

    public struct InstanceData
    {
        public uint Type;
        public Matrix4x4 World;
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 Center;
        public float Radius;

        public InstanceData(uint type, Matrix4x4 world, Vector3 min, Vector3 max, Vector3 center, float radius)
        {
            Type = type;
            World = world;
            Min = min;
            Max = max;
            Center = center;
            Radius = radius;
        }
    }
}