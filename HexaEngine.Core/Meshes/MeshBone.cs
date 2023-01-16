namespace HexaEngine.Meshes
{
    using HexaEngine.Core.Meshes;
    using HexaEngine.Scenes;
    using System.Numerics;

    public unsafe struct MeshBone
    {
        public string Name;
        public Animature* Animature;
        public GameObject Node;
        public MeshWeight[] Weights;
        public Matrix4x4 Offset;

        public MeshBone(string name, Animature* animature, GameObject node, MeshWeight[] weights, Matrix4x4 offset)
        {
            Name = name;
            Animature = animature;
            Node = node;
            Weights = weights;
            Offset = offset;
        }
    }
}