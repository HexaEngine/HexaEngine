namespace HexaEngine.Scenes.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using System.Numerics;

    [EditorComponent<TriangleCollider>("Triangle Collider")]
    public class TriangleCollider : BaseCollider
    {
        private Vector3 pos1;
        private Vector3 pos2;
        private Vector3 pos3;

        [EditorProperty("Pos 1")]
        public Vector3 Pos1
        { get => pos1; set { pos1 = value; update = true; } }

        [EditorProperty("Pos 2")]
        public Vector3 Pos2
        { get => pos2; set { pos2 = value; update = true; } }

        [EditorProperty("Pos 3")]
        public Vector3 Pos3
        { get => pos3; set { pos3 = value; update = true; } }

        public override void CreateShape()
        {
            if (Application.InDesignMode || parent == null || scene == null || hasShape) return;
            Triangle triangle = new(pos1, pos2, pos3);
            pose = new(parent.Transform.GlobalPosition, parent.Transform.GlobalOrientation);
            inertia = triangle.ComputeInertia(Mass);
            index = scene.Simulation.Shapes.Add(triangle);
            hasShape = true;
        }
    }
}