namespace HexaEngine.Scenes.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;

    [EditorComponent<CapsuleCollider>("Capsule Collider")]
    public class CapsuleCollider : BaseCollider
    {
        private float radius = 1;
        private float length = 2;

        [EditorProperty("Radius")]
        public float Radius
        { get => radius; set { radius = value; update = true; } }

        [EditorProperty("Length")]
        public float Length
        { get => length; set { length = value; update = true; } }

        public override void CreateShape()
        {
            if (Application.InDesignMode || parent == null || simulation == null || hasShape)
            {
                return;
            }

            Capsule capsule = new(radius, length);
            pose = new(parent.Transform.GlobalPosition, parent.Transform.GlobalOrientation);
            inertia = capsule.ComputeInertia(Mass);
            index = simulation.Shapes.Add(capsule);
            hasShape = true;
        }
    }
}