namespace HexaEngine.Components.Collider
{
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;

    [EditorCategory("Collider")]
    [EditorComponent<SphereCollider>("Sphere Collider")]
    public unsafe class SphereCollider : BaseCollider
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