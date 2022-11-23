namespace HexaEngine.Plugins2.Records
{
    using HexaEngine.Scenes;
    using System;

    public struct MeshMaterialIndex
    {
        public string MaterialIndex;
        public string MeshIndex;
    }

    public class NodeRecord : Record
    {
        public NodeRecord(SceneNode node) : base(node.Name)
        {
            Transform transform = new();
            transform.POS = node.Transform.PositionRotationScale;
            Transform = transform;
            Scene scene = node.GetScene();
            Meshes = new MeshMaterialIndex[node.Meshes.Count];
            for (int i = 0; i < node.Meshes.Count; i++)
            {
                var index = node.Meshes[i];
                var mesh = scene.Meshes[index];
                Meshes[i] = new() { MaterialIndex = mesh.MaterialName, MeshIndex = mesh.Name };
            }
        }

        public Transform Transform;
        public MeshMaterialIndex[] Meshes;

        public override RecordType Type => RecordType.Node;

        public override int Read(ReadOnlySpan<byte> source)
        {
            throw new NotImplementedException();
        }

        public override int Size()
        {
            throw new NotImplementedException();
        }

        public override int Write(Span<byte> source)
        {
            throw new NotImplementedException();
        }
    }
}