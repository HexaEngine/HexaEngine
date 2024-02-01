namespace HexaEngine.Components.Physics
{
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Physics;
    using MagicPhysX;
    using System.Numerics;

    [EditorCategory("Joints", "Physics")]
    [EditorComponent<FixedJoint>("Fixed Joint")]
    public unsafe class FixedJoint : Joint
    {
        private PxFixedJoint* joint;

        protected override unsafe PxJoint* CreateJoint(PxPhysics* physics, PxScene* scene, RigidBody rigidBody0, RigidBody rigidBody1, Transform transform0, Transform transform1)
        {
            var relativePos = rigidBody1.GameObject.Transform.GlobalPosition - GameObject.Transform.GlobalPosition;

            var midPoint = relativePos / 2f;

            PxTransform local0 = Helper.Convert(midPoint, Quaternion.Identity);
            PxTransform local1 = Helper.Convert(-midPoint, Quaternion.Identity);

            joint = physics->PhysPxFixedJointCreate(rigidBody0.Actor, &local0, rigidBody1.Actor, &local1);
            return (PxJoint*)joint;
        }
    }
}