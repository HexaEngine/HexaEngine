namespace HexaEngine.Physics.Joints
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Physics;
    using HexaEngine.Scenes.Serialization;
    using MagicPhysX;
    using System.Numerics;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Physics.SliderJoint")]
    [EditorCategory("Joints", "Physics")]
    [EditorComponent<SliderJoint>("Slider Joint")]
    public sealed unsafe class SliderJoint : Joint
    {
        private PxPrismaticJoint* joint;

        private bool autoCalculate;
        private Vector3 anchor;
        private Vector3 connectedAnchor;
        private bool enableLimit;
        private float limitMin;
        private float limitMax;
        private float limitRestitution;
        private float limitBounceThreshold;
        private float springStiffness;
        private float springDamping;

        [EditorProperty("Auto Calculate")]
        public bool AutoCalculate
        {
            get => autoCalculate;
            set
            {
                autoCalculate = value;
            }
        }

        [EditorProperty("Anchor")]
        public Vector3 Anchor
        {
            get => anchor;
            set
            {
                anchor = value;
            }
        }

        [EditorProperty("Connected Anchor")]
        public Vector3 ConnectedAnchor
        {
            get => connectedAnchor;
            set
            {
                connectedAnchor = value;
            }
        }

        [JsonIgnore]
        public float Position
        {
            get
            {
                if (joint == null || updating)
                {
                    return 0.0f;
                }

                return joint->GetPosition();
            }
        }

        [JsonIgnore]
        public float Velocity
        {
            get
            {
                if (joint == null || updating)
                {
                    return 0.0f;
                }

                return joint->GetVelocity();
            }
        }

        [EditorCategory("Spring")]
        [EditorProperty("Stiffness")]
        public float SpringStiffness
        {
            get => springStiffness;
            set
            {
                springStiffness = value;
                UpdateLimits();
            }
        }

        [EditorCategory("Spring")]
        [EditorProperty("Damping")]
        public float SpringDamping
        {
            get => springDamping;
            set
            {
                springDamping = value;
                UpdateLimits();
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Enable Limits")]
        public bool EnableLimit
        {
            get => enableLimit;
            set
            {
                enableLimit = value;

                if (joint != null && !updating)
                {
                    joint->SetPrismaticJointFlagMut(PxPrismaticJointFlag.LimitEnabled, value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Max")]
        public float LimitMax
        {
            get => limitMax;
            set
            {
                limitMax = value;

                UpdateLimits();
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Min")]
        public float LimitMin
        {
            get => limitMin;
            set
            {
                limitMin = value;

                UpdateLimits();
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Restitution")]
        public float LimitRestitution
        {
            get => limitRestitution;
            set
            {
                limitRestitution = value;

                UpdateLimits();
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Bounce Threshold")]
        public float LimitBounceThreshold
        {
            get => limitBounceThreshold;
            set
            {
                limitBounceThreshold = value;

                UpdateLimits();
            }
        }

        protected override unsafe PxJoint* CreateJoint(PxPhysics* physics, PxScene* scene, RigidBody rigidBody0, RigidBody rigidBody1, Transform transform0, Transform transform1)
        {
            Vector3 connectedAnchor = AutoCalculate ? transform0.GlobalPosition + Anchor - transform1.GlobalPosition : ConnectedAnchor;

            // Convert local transforms to PxTransform objects
            PxTransform local0 = new() { p = anchor, q = Quaternion.Identity };
            PxTransform local1 = new() { p = connectedAnchor, q = Quaternion.Identity };

            joint = physics->PhysPxPrismaticJointCreate(rigidBody0.Actor, &local0, rigidBody1.Actor, &local1);
            joint->SetPrismaticJointFlagMut(PxPrismaticJointFlag.LimitEnabled, enableLimit);

            UpdateLimits();

            return (PxJoint*)joint;
        }

        private void UpdateLimits()
        {
            if (joint == null || updating)
            {
                return;
            }

            PxJointLinearLimitPair limit;
            limit.lower = limitMin;
            limit.upper = limitMax;
            limit.restitution = limitRestitution;
            limit.bounceThreshold = limitBounceThreshold;
            limit.stiffness = springStiffness;
            limit.damping = springDamping;

            joint->SetLimitMut(&limit);
        }
    }
}