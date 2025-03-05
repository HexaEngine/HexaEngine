namespace HexaEngine.Physics.Joints
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Physics;
    using HexaEngine.Scenes.Serialization;
    using MagicPhysX;
    using System.Numerics;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Physics.FixedJoint")]
    [EditorCategory("Joints", "Physics")]
    [EditorComponent<FixedJoint>("Fixed Joint", Icon = "\xf0c1")]
    public sealed unsafe class FixedJoint : Joint
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