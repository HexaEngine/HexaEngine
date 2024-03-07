namespace HexaEngine.Components.Physics.Collider
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using MagicPhysX;
    using System.Numerics;

    [EditorCategory("Collider", "Physics")]
    [EditorComponent<CapsuleCollider>("Capsule Collider")]
    public unsafe class CapsuleCollider : ColliderShape
    {
        private float radius = 1;
        private float length = 1;

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

            localPose.q = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathUtil.PIDIV2) * localPose.q;

            AttachShape(actor, shape, localPose);
        }
    }
}