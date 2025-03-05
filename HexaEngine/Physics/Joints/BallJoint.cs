namespace HexaEngine.Physics.Joints
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Physics;
    using HexaEngine.Scenes.Serialization;
    using MagicPhysX;
    using System.Numerics;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Physics.BallJoint")]
    [EditorCategory("Joints", "Physics")]
    [EditorComponent<BallJoint>("Ball Joint", Icon = "\xf0c1")]
    public sealed unsafe class BallJoint : Joint
    {
        private PxSphericalJoint* joint;
        private bool autoCalculate;
        private Vector3 anchor;
        private Vector3 connectedAnchor;
        private bool enableLimit;
        private float springStiffness;
        private float springDamping;
        private float limitRestitution;
        private float limitBounceThreshold;
        private float limitYAngle = MathUtil.PIDIV2;
        private float limitZAngle = MathUtil.PIDIV2;

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
        [EditorProperty("Enable Limit")]
        public bool EnableLimit
        {
            get => enableLimit;
            set
            {
                enableLimit = value;

                if (joint != null && !updating)
                {
                    joint->SetSphericalJointFlagMut(PxSphericalJointFlag.LimitEnabled, value);
                }
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
        [EditorProperty("Y Angle", 0, MathUtil.PI, EditorPropertyMode.SliderAngle)]
        public float LimitYAngle
        {
            get => limitYAngle;
            set
            {
                limitYAngle = value;

                UpdateLimits();
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Z Angle", 0, MathUtil.PI, EditorPropertyMode.SliderAngle)]
        public float LimitZAngle
        {
            get => limitZAngle;
            set
            {
                limitZAngle = value;

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

        [JsonIgnore]
        public float SwingAngleY
        {
            get
            {
                if (joint == null || updating)
                {
                    return 0;
                }

                return joint->GetSwingYAngle();
            }
        }

        [JsonIgnore]
        public float SwingAngleZ
        {
            get
            {
                if (joint == null || updating)
                {
                    return 0;
                }

                return joint->GetSwingZAngle();
            }
        }

        protected override unsafe PxJoint* CreateJoint(PxPhysics* physics, PxScene* scene, RigidBody rigidBody0, RigidBody rigidBody1, Transform transform0, Transform transform1)
        {
            Vector3 connectedAnchor = AutoCalculate ? transform0.GlobalPosition + Anchor - transform1.GlobalPosition : ConnectedAnchor;

            // Convert local transforms to PxTransform objects
            PxTransform local0 = new() { p = anchor, q = Quaternion.Identity };
            PxTransform local1 = new() { p = connectedAnchor, q = Quaternion.Identity };

            joint = physics->PhysPxSphericalJointCreate(rigidBody0.Actor, &local0, rigidBody1.Actor, &local1);
            joint->SetSphericalJointFlagMut(PxSphericalJointFlag.LimitEnabled, enableLimit);

            UpdateLimits();

            return (PxJoint*)joint;
        }

        private void UpdateLimits()
        {
            if (joint == null || updating)
            {
                return;
            }

            PxJointLimitCone limit;

            limit.yAngle = limitYAngle;
            limit.zAngle = limitZAngle;
            limit.restitution = limitRestitution;
            limit.bounceThreshold = limitBounceThreshold;
            limit.stiffness = springStiffness;
            limit.damping = springDamping;

            joint->SetLimitConeMut(&limit);
        }
    }
}