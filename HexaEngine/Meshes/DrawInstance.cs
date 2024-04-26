namespace HexaEngine.Meshes
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class DrawInstance
    {
        public uint InstanceId;
        public int NodeId;
        public Matrix4x4 Transform;
        public BoundingSphere BoundingSphere;

        public DrawInstance(int nodeId)
        {
            NodeId = nodeId;
        }

        public DrawInstance(int nodeId, Matrix4x4 transform, BoundingSphere boundingSphere)
        {
            NodeId = nodeId;
            Transform = transform;
            BoundingSphere = boundingSphere;
        }
    }
}