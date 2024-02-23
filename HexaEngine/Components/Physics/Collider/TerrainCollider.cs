namespace HexaEngine.Components.Physics.Collider
{
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;
    using System;
    using System.Numerics;

    [EditorCategory("Collider", "Physics")]
    [EditorComponent<TerrainCollider>("Terrain Collider", false, true)]
    public unsafe class TerrainCollider : BaseCollider
    {
        public override unsafe void AddShapes(PxPhysics* physics, PxScene* scene, PxRigidActor* actor, PxTransform localPose, Vector3 scale)
        {
            throw new NotImplementedException();
        }
    }
}