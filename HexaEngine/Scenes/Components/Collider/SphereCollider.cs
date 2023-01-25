namespace HexaEngine.Scenes.Components
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;

    [EditorComponent<SphereCollider>("Sphere Collider")]
    public class SphereCollider : BaseCollider
    {
        private float radius = 1;

        [EditorProperty("Radius")]
        public float Radius
        { get => radius; set { radius = value; update = true; } }

        public override void CreateShape()
        {
            if (Application.InDesignMode || parent == null || scene == null || hasShape) return;
            Sphere sphere = new(radius);
            pose = new(parent.Transform.GlobalPosition, parent.Transform.GlobalOrientation);
            inertia = sphere.ComputeInertia(Mass);
            index = scene.Simulation.Shapes.Add(sphere);
            hasShape = true;
        }
    }
}