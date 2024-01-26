namespace HexaEngine.Components.Collider.PhysX
{
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;

    [EditorCategory("Collider (PhysX)")]
    [EditorComponent<PhysXCapsuleCollider>("Capsule Collider (PhysX)")]
    public unsafe class PhysXCapsuleCollider : PhysXBaseCollider
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