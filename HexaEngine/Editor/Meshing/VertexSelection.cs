namespace HexaEngine.Editor.Meshes
{
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using ImGuiNET;
    using System.Numerics;

    public class VertexSelection
    {
        private readonly List<uint> indices = new();

        public int Count => indices.Count;

        public bool IsEmpty => indices.Count == 0;

        public Vector3 ComputeCenter(MeshData data)
        {
            Vector3 sum = default;

            for (int i = 0; i < Count; i++)
            {
                var vertex = indices[i];
                sum += data.Positions[vertex];
            }

            return sum / Count;
        }

        public Vector3 ComputeNormal(MeshData data)
        {
            Vector3 sum = default;

            for (int i = 0; i < Count; i++)
            {
                var vertex = indices[i];
                sum += data.Normals[vertex];
            }

            return sum / Count;
        }

        public void Transform(MeshData data, Vector3 center, Matrix4x4 transform)
        {
            for (int i = 0; i < Count; i++)
            {
                var vertex = indices[i];
                data.Positions[vertex] = Vector3.Transform(data.Positions[vertex] - center, transform);
            }
            data.GenerateNTB();
        }

        public void OverrideAdd(uint id)
        {
            indices.Clear();
            indices.Add(id);
        }

        public void Add(uint id)
        {
            if (!indices.Contains(id))
            {
                indices.Add(id);
            }
        }

        public void Remove(uint id)
        {
            indices.Remove(id);
        }

        public void Clear()
        {
            indices.Clear();
        }
    }
}