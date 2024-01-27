namespace HexaEngine.Components.Collider
{
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;

    [EditorCategory("Collider")]
    [EditorComponent<PlaneCollider>("Plane Collider")]
    public unsafe class PlaneCollider : BaseCollider
    {
        public override unsafe void CreateShape()
        {
            var plane = NativeMethods.PxPlaneGeometry_new();
            shape = pxPhysics->CreateShapeMut((PxGeometry*)&plane, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);
        }
    }
}