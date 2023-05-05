namespace HexaEngine.Scenes.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;

    [EditorComponent<CylinderCollider>("Cylinder Collider")]
    public class CylinderCollider : BaseCollider
    {
        private float radius = 1;
        private float length = 1;

        [EditorProperty("Radius")]
        public float Radius
        { get => radius; set { radius = value; update = true; } }

        [EditorProperty("Length")]
        public float Length
        { get => length; set { length = value; update = true; } }

        public override void CreateShape()
        {
            if (Application.InDesignMode || parent == null || simulation == null || bufferPool == null || hasShape)
            {
                return;
            }

            Cylinder cylinder = new(radius, length * 2);
            pose = new(parent.Transform.GlobalPosition, parent.Transform.GlobalOrientation);
            index = simulation.Shapes.Add(cylinder);
            inertia = cylinder.ComputeInertia(Mass);
            hasShape = true;
        }
    }
}