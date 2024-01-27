namespace HexaEngine.Components.Collider
{
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;

    [EditorCategory("Collider")]
    [EditorComponent<BoxCollider>("Box Collider")]
    public unsafe class BoxCollider : BaseCollider
    {
        private float height = 1;
        private float depth = 1;
        private float width = 1;

        [EditorProperty("Width")]
        public float Width
        { get => width; set { width = value; update = true; } }

        [EditorProperty("Height")]
        public float Height
        { get => height; set { height = value; update = true; } }

        [EditorProperty("Depth")]
        public float Depth
        { get => depth; set { depth = value; update = true; } }

        public override void CreateShape()
        {
            var box = NativeMethods.PxBoxGeometry_new(width, height, depth);
            shape = pxPhysics->CreateShapeMut((PxGeometry*)&box, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);
        }
    }
}