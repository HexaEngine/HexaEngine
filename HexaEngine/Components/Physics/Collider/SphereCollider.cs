namespace HexaEngine.Components.Physics.Collider
{
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;
    using System.Numerics;

    [EditorCategory("Collider", "Physics")]
    [EditorComponent<SphereCollider>("Sphere Collider")]
    public unsafe class SphereCollider : BaseCollider
    {
        private float radius = 1;

        [EditorProperty("Radius")]
        public float Radius
        { get => radius; set { radius = value; } }

        public override unsafe void AddShapes(PxPhysics* physics, PxScene* scene, PxRigidActor* actor, PxTransform localPose, Vector3 scale)
        {
            var sphere = NativeMethods.PxSphereGeometry_new(radius);
            var shape = physics->CreateShapeMut((PxGeometry*)&sphere, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);
            AttachShape(actor, shape);
        }
    }
}