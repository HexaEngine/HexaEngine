namespace HexaEngine.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Editor.Attributes;

    [EditorCategory("Collider")]
    [EditorComponent<SphereCollider>("Sphere Collider")]
    public class SphereCollider : BepuBaseCollider
    {
        private float radius = 1;

        [EditorProperty("Radius")]
        public float Radius
        { get => radius; set { radius = value; update = true; } }

        public override void CreateShape()
        {
            if (Application.InDesignMode || GameObject == null || simulation == null || bufferPool == null || hasShape)
            {
                return;
            }

            Sphere sphere = new(radius);
            pose = new(GameObject.Transform.GlobalPosition, GameObject.Transform.GlobalOrientation);
            inertia = sphere.ComputeInertia(Mass);
            lock (simulation)
            {
                index = simulation.Shapes.Add(sphere);
            }
            hasShape = true;
        }
    }
}