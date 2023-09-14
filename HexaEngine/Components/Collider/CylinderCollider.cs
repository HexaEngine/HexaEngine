namespace HexaEngine.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Editor.Attributes;

    [EditorComponent<CylinderCollider>("Cylinder Collider")]
    public class CylinderCollider : BaseCollider
    {
        private float radius = 1;
        private float length = 1;

        [EditorProperty("FilterRadius")]
        public float Radius
        { get => radius; set { radius = value; update = true; } }

        [EditorProperty("Length")]
        public float Length
        { get => length; set { length = value; update = true; } }

        public override void CreateShape()
        {
            if (Application.InDesignMode || GameObject == null || simulation == null || bufferPool == null || hasShape)
            {
                return;
            }

            Cylinder cylinder = new(radius, length * 2);
            pose = new(GameObject.Transform.GlobalPosition, GameObject.Transform.GlobalOrientation);
            index = simulation.Shapes.Add(cylinder);
            inertia = cylinder.ComputeInertia(Mass);
            hasShape = true;
        }
    }
}