namespace HexaEngine.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Editor.Attributes;
    using System.Numerics;

    [EditorComponent(typeof(ConvexHullCollider), "Convex Hull Collider")]
    public class ConvexHullCollider : BaseCollider
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

            ConvexHullHelper.CreateShape(points, bufferPool, out var dat, out center, out var ull);
            hullData = dat;

            convexHull = ull;
        }

        public override void CreateShape()
        {
            if (Application.InDesignMode || parent == null || simulation == null || convexHull == null || hasShape)
            {
                return;
            }

            var ull = convexHull.Value;
            inertia = ull.ComputeInertia(Mass);
            pose = new(parent.Transform.GlobalPosition, parent.Transform.GlobalOrientation);
            index = simulation.Shapes.Add(ull);
            hasShape = true;
        }

        public override void DestroyShape()
        {
            if (Application.InDesignMode || parent == null || simulation == null || bufferPool == null || !hasShape)
            {
                return;
            }

            simulation.Shapes.Remove(index);
            hasShape = false;
            convexHull?.Dispose(bufferPool);
        }
    }
}