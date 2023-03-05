namespace HexaEngine.Scenes.Components.Collider
{
    using BepuPhysics.Collidables;
    using BepuUtilities.Memory;
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Meshes;
    using System;
    using System.Numerics;

    [EditorComponent(typeof(MeshCollider), "Mesh Collider")]
    public class MeshCollider : BaseCollider
    {
        private string meshPath = string.Empty;
        private Mesh mesh;
        private Vector3 scale = Vector3.One;

        [EditorProperty("Mesh")]
        public string MeshPath
        { get => meshPath; set { meshPath = value; update = true; } }

        [EditorProperty("Scale")]
        public Vector3 Scale { get => scale; set => scale = value; }

        public override void CreateShape()
        {
            if (parent == null || scene == null || bufferPool == null || simulation == null || hasShape) return;
            var data = scene.MeshManager.Load(Paths.CurrentAssetsPath + meshPath);
            ulong vertexCount = 0;
            for (ulong i = 0; i < data.Header.MeshCount; i++)
            {
                var meh = data.GetMesh(i);
                vertexCount += meh.IndicesCount;
            }

            bufferPool.Take((int)vertexCount / 3, out Buffer<Triangle> buffer);
            int m = 0;
            for (ulong i = 0; i < data.Header.MeshCount; i++)
            {
                var meh = data.GetMesh(i);
                for (int j = 0; j < meh.Indices.Length; j += 3)
                {
                    // Note verts are loaded counter-clockwise because the engine operates in LH mode
                    MeshVertex a = meh.Vertices[meh.Indices[j + 2]];
                    MeshVertex b = meh.Vertices[meh.Indices[j + 1]];
                    MeshVertex c = meh.Vertices[meh.Indices[j]];
                    buffer[m++] = new(a.Position, b.Position, c.Position);
                }
            }
            mesh = new(buffer, scale, bufferPool);
            inertia = mesh.ComputeClosedInertia(Mass);
            pose = new(parent.Transform.GlobalPosition, parent.Transform.GlobalOrientation);
            index = simulation.Shapes.Add(mesh);
            hasShape = true;
        }

        public override void DestroyShape()
        {
            if (Application.InDesignMode || parent == null || simulation == null || bufferPool == null || !hasShape) return;
            simulation.Shapes.Remove(index);
            hasShape = false;
            mesh.Dispose(bufferPool);
        }
    }
}