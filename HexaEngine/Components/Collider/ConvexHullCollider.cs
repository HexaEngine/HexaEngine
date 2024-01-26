namespace HexaEngine.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Editor.Attributes;
    using System.Numerics;

    [EditorCategory("Collider")]
    [EditorComponent(typeof(ConvexHullCollider), "Convex Hull Collider")]
    public class ConvexHullCollider : BepuBaseCollider
    {
        private string mesh = string.Empty;
        private ConvexHull? convexHull;
        private HullData? hullData;
        private Vector3 center;
        private Vector3[]? points;

        [EditorProperty("Mesh")]
        public string Mesh
        { get => mesh; set { mesh = value; update = true; } }

        [JsonIgnore]
        public ConvexHull? ConvexHull => convexHull;

        [JsonIgnore]
        public HullData? HullData => hullData;

        [JsonIgnore]
        public Vector3[]? Points => points;

        [JsonIgnore]
        public Vector3 Center => center;

        [EditorButton("Bake shape")]
        public void BakeShape()
        {
            if (bufferPool == null || scene == null)
            {
                return;
            }

            var data = scene.ModelManager.Load(mesh);
            points = data.GetAllPoints();

            lock (bufferPool)
            {
                ConvexHullHelper.CreateShape(points, bufferPool, out var dat, out center, out var ull);

                hullData = dat;

                convexHull = ull;
            }
        }

        public override void CreateShape()
        {
            if (Application.InDesignMode || GameObject == null || simulation == null || convexHull == null || hasShape)
            {
                return;
            }

            var ull = convexHull.Value;
            inertia = ull.ComputeInertia(Mass);
            pose = new(GameObject.Transform.GlobalPosition, GameObject.Transform.GlobalOrientation);
            lock (simulation)
            {
                index = simulation.Shapes.Add(ull);
            }
            hasShape = true;
        }

        public override void DestroyShape()
        {
            if (Application.InDesignMode || GameObject == null || simulation == null || bufferPool == null || !hasShape)
            {
                return;
            }
            lock (simulation)
            {
                simulation.Shapes.Remove(index);
            }
            hasShape = false;
            lock (bufferPool)
            {
                convexHull?.Dispose(bufferPool);
            }
        }
    }
}