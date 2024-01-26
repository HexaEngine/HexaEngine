namespace HexaEngine.Components.Collider.PhysX
{
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;

    [EditorCategory("Collider (PhysX)")]
    [EditorComponent<PhysXPlaneCollider>("Plane Collider (PhysX)")]
    public unsafe class PhysXPlaneCollider : PhysXBaseCollider
    {
        public override unsafe void CreateShape()
        {
            var plane = NativeMethods.PxPlaneGeometry_new();
            shape = pxPhysics->CreateShapeMut((PxGeometry*)&plane, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);
        }
    }
}