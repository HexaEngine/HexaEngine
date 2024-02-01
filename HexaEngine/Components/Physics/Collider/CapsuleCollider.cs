namespace HexaEngine.Components.Physics.Collider
{
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;
    using System.Numerics;

    [EditorCategory("Collider", "Physics")]
    [EditorComponent<CapsuleCollider>("Capsule Collider")]
    public unsafe class CapsuleCollider : BaseCollider
    {
        private float radius = 1;
        private float length = 2;

        [EditorProperty("Radius")]
        public float Radius
        { get => radius; set { radius = value; } }

        [EditorProperty("Length")]
        public float Length
        { get => length; set { length = value; } }

        public override unsafe void AddShapes(PxPhysics* physics, PxScene* scene, PxRigidActor* actor, PxTransform localPose, Vector3 scale)
        {
            var box = NativeMethods.PxCapsuleGeometry_new(radius, length);
            var shape = physics->CreateShapeMut((PxGeometry*)&box, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);
            AttachShape(actor, shape);
        }
    }
}