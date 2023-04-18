namespace HexaEngine.Scenes.Components.Collider
{
    using BepuPhysics.Collidables;
    using BepuUtilities.Memory;
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
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
            var data = scene.ModelManager.Load(Paths.CurrentAssetsPath + meshPath);
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
                    Vector3 a = meh.Positions[meh.Indices[j + 2]];
                    Vector3 b = meh.Positions[meh.Indices[j + 1]];
                    Vector3 c = meh.Positions[meh.Indices[j]];
                    buffer[m++] = new(a, b, c);
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