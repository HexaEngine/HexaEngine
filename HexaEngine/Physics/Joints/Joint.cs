namespace HexaEngine.Physics.Joints
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Physics;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Serialization;
    using MagicPhysX;
    using System;
    using System.Numerics;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Physics.Joint")]
    public abstract unsafe class Joint : IJointComponent
    {
        private PxJoint* joint;
        private byte* name;
        protected bool updating = false;
        private bool collisionEnabled;
        private float breakForce = float.PositiveInfinity;
        private float breakTorque = float.PositiveInfinity;

        /// <summary>
        /// The GUID of the <see cref="Joint"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        public Guid Guid { get; set; } = Guid.NewGuid();

        [JsonIgnore]
        public bool IsSerializable { get; } = true;

        public GameObject GameObject { get; set; }

        public event Action<IJointComponent>? OnRecreate;

        [EditorProperty("Connected Object")]
        public GameObject? ConnectedObject { get; set; }

        [EditorProperty("Break Force")]
        public float BreakForce
        {
            get => breakForce;
            set
            {
                value = MathUtil.Clamp(value, 0, float.PositiveInfinity);

                breakForce = value;

                if (joint != null && !updating)
                {
                    joint->SetBreakForceMut(value, breakTorque);
                }
            }
        }

        [EditorProperty("Break Torque")]
        public float BreakTorque
        {
            get => breakTorque;
            set
            {
                value = MathUtil.Clamp(value, 0, float.PositiveInfinity);

                breakTorque = value;

                if (joint != null && !updating)
                {
                    joint->SetBreakForceMut(breakForce, value);
                }
            }
        }

        [EditorProperty("Collision Enabled")]
        public bool CollisionEnabled
        {
            get => collisionEnabled;
            set
            {
                collisionEnabled = value;

                if (joint != null && !updating)
                {
                    joint->SetConstraintFlagMut(PxConstraintFlag.CollisionEnabled, value);
                }
            }
        }

        [JsonIgnore]
        public bool IsBroken
        {
            get
            {
                if (joint == null || updating)
                {
                    return default;
                }

                return (joint->GetConstraintFlags() & PxConstraintFlags.Broken) != 0;
            }
            set
            {
                if (joint == null || updating)
                {
                    return;
                }

                joint->SetConstraintFlagMut(PxConstraintFlag.Broken, value);
            }
        }

        [JsonIgnore]
        public Vector3 RelativeLinearVelocity
        {
            get
            {
                if (joint == null || updating)
                {
                    return default;
                }

                return joint->GetRelativeLinearVelocity();
            }
        }

        [JsonIgnore]
        public Vector3 RelativeAngularVelocity
        {
            get
            {
                if (joint == null || updating)
                {
                    return default;
                }

                return joint->GetRelativeAngularVelocity();
            }
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Destroy()
        {
        }

        void IComponent.Awake()
        {
            Awake();
        }

        void IComponent.Destroy()
        {
            Destroy();
        }

        private void NotifyOnRecreate()
        {
            updating = true;
            OnRecreate?.Invoke(this);
        }

        protected abstract PxJoint* CreateJoint(PxPhysics* physics, PxScene* scene, RigidBody rigidBody0, RigidBody rigidBody1, Transform transform0, Transform transform1);

        void IJointComponent.CreateJoint(PxPhysics* physics, PxScene* scene)
        {
            RigidBody? rigidBody0 = GameObject.GetComponent<RigidBody>();
            RigidBody? rigidBody1 = ConnectedObject?.GetComponent<RigidBody>();

            if (rigidBody0 == null || rigidBody1 == null)
            {
                return;
            }

            var transform0 = GameObject.Transform;
            var transform1 = rigidBody1.GameObject.Transform;

            joint = CreateJoint(physics, scene, rigidBody0, rigidBody1, transform0, transform1);
            if (joint != null)
            {
                joint->SetBreakForceMut(breakForce, breakTorque);
                joint->SetConstraintFlagMut(PxConstraintFlag.CollisionEnabled, collisionEnabled);

                var constraint = joint->GetConstraint();

                if (joint == null)
                {
                    return;
                }

                name = $"{GameObject.FullName}+Joint".ToUTF8Ptr();

                joint->SetNameMut(name);

                updating = false;
            }
        }

        void IJointComponent.DestroyJoint()
        {
            DestroyJoint();
        }

        protected virtual void DestroyJoint()
        {
            if (joint != null)
            {
                var scene = joint->GetScene();
                joint->SetNameMut(null);
                joint->ReleaseMut();
                joint = null;
            }

            if (name != null)
            {
                Free(name);
                name = null;
            }
        }

        public void SetLocalPose(int index, Vector3 position, Quaternion rotation)
        {
            if (joint == null || updating)
            {
                return;
            }

            PxTransform localPose = Helper.Convert(position, rotation);
            joint->SetLocalPoseMut((PxJointActorIndex)index, &localPose);
        }

        public void GetLocalPose(int index, out Vector3 position, out Quaternion rotation)
        {
            if (joint == null || updating)
            {
                position = default;
                rotation = default;
                return;
            }

            PxTransform localPose = joint->GetLocalPose((PxJointActorIndex)index);
            position = localPose.p;
            rotation = localPose.q;
        }

        public void GetRelativeTransform(out Vector3 position, out Quaternion rotation)
        {
            if (joint == null || updating)
            {
                position = default;
                rotation = default;
                return;
            }

            PxTransform transform = joint->GetRelativeTransform();
            position = transform.p;
            rotation = transform.q;
        }
    }
}