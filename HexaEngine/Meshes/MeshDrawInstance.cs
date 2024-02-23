namespace HexaEngine.Meshes
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public struct MeshDrawInstance
    {
        public int NodeId;
        public Matrix4x4 Transform;
        public BoundingBox BoundingBox;

        public MeshDrawInstance(int nodeId)
        {
            NodeId = nodeId;
        }

        public MeshDrawInstance(int nodeId, Matrix4x4 transform, BoundingBox boundingBox)
        {
            NodeId = nodeId;
            Transform = transform;
            BoundingBox = boundingBox;
        }
    }
}