namespace HexaEngine.Components.Collider
{
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;

    [EditorCategory("Collider")]
    [EditorComponent<CapsuleCollider>("Capsule Collider")]
    public unsafe class CapsuleCollider : BaseCollider
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
            var capsule = NativeMethods.PxCapsuleGeometry_new(radius, length);
            shape = pxPhysics->CreateShapeMut((PxGeometry*)&capsule, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);
        }
    }
}