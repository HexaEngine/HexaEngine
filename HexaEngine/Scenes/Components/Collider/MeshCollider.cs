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

        [EditorProperty("Mesh")]
        public string MeshPath
        { get => meshPath; set { meshPath = value; update = true; } }

        public override void CreateShape()
        {
            if (parent == null || scene == null || bufferPool == null || simulation == null || hasShape) return;
            var data = scene.MeshManager.Load(meshPath);
            var meh = data.ReadMesh();
            bufferPool.Take(meh.Indices.Length, out Buffer<Triangle> buffer);
            for (int i = 0; i < meh.Indices.Length; i += 3)
            {
                // Note verts are loaded counter-clockwise because the engine operates in LH mode
                MeshVertex a = meh.Vertices[meh.Indices[i + 2]];
                MeshVertex b = meh.Vertices[meh.Indices[i + 1]];
                MeshVertex c = meh.Vertices[meh.Indices[i]];
                buffer[i] = new(a.Position, b.Position, c.Position);
            }
            mesh = new(buffer, Vector3.One, bufferPool);
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