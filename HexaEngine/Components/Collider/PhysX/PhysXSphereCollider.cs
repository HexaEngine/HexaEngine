namespace HexaEngine.Components.Collider.PhysX
{
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;

    [EditorCategory("Collider (PhysX)")]
    [EditorComponent<PhysXSphereCollider>("Sphere Collider (PhysX)")]
    public unsafe class PhysXSphereCollider : PhysXBaseCollider
    {
        private float radius = 1;

        [EditorProperty("Radius")]
        public float Radius
        { get => radius; set { radius = value; update = true; } }

        public override unsafe void CreateShape()
        {
            var sphere = NativeMethods.PxSphereGeometry_new(radius);
            shape = pxPhysics->CreateShapeMut((PxGeometry*)&sphere, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);
        }
    }
}