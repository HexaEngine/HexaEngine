namespace HexaEngine.Meshes
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public struct MeshDrawType
    {
        public uint MeshId;
        public uint TypeId;
        public uint DrawIndirectOffset;
        public MeshDrawInstance[] Instances;

        public MeshDrawType(uint meshId, uint typeId, MeshDrawInstance[] instances)
        {
            MeshId = meshId;
            TypeId = typeId;
            Instances = instances;
        }
    }

    public class DrawInstance
    {
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

    public class DrawType
    {
        public uint MeshId;
        public uint TypeId;
        public uint DrawIndirectOffset;
        public DrawInstance[] Instances;

        public DrawType(uint meshId, uint typeId, DrawInstance[] instances)
        {
            MeshId = meshId;
            TypeId = typeId;
            Instances = instances;
        }
    }
}