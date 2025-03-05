namespace HexaEngine.Components.Physics.Collider
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes.Serialization;
    using MagicPhysX;
    using System.Numerics;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Physics.Collider.PlaneCollider")]
    [EditorCategory("Collider", "Physics")]
    [EditorComponent<PlaneCollider>("Plane Collider", Icon = "\xf61f")]
    public unsafe class PlaneCollider : ColliderShape
    {
        public override unsafe void AddShapes(PxPhysics* physics, PxScene* scene, PxRigidActor* actor, PxTransform localPose, Vector3 scale)
        {
            var plane = NativeMethods.PxPlaneGeometry_new();
            var shape = physics->CreateShapeMut((PxGeometry*)&plane, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);
            AttachShape(actor, shape);
        }
    }
}