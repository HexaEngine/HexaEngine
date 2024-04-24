namespace HexaEngine.Components.Physics.Collider
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes.Serialization;
    using MagicPhysX;
    using System.Numerics;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Physics.Collider.SphereCollider")]
    [EditorCategory("Collider", "Physics")]
    [EditorComponent<SphereCollider>("Sphere Collider")]
    public unsafe class SphereCollider : ColliderShape
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