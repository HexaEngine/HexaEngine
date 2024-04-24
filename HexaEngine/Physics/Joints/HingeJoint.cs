namespace HexaEngine.Physics.Joints
{
    using HexaEngine.Core;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Physics;
    using HexaEngine.Scenes.Serialization;
    using MagicPhysX;
    using System.Numerics;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Physics.HingeJoint")]
    [EditorCategory("Joints", "Physics")]
    [EditorComponent<HingeJoint>("Hinge Joint")]
    public sealed unsafe class HingeJoint : Joint
    {
        private PxRevoluteJoint* joint;
        private Vector3 anchor = Vector3.Zero;
        private Vector3 axis = new(1, 0, 0);
        private Vector3 connectedAnchor = Vector3.Zero;
        private bool autoCalculate = true;
        private bool enableLimit = false;
        private float limitMin = -MathUtil.PIDIV2;
        private float limitMax = MathUtil.PIDIV2;
        private float limitRestitution;
        private float limitBounceThreshold;
        private float springStiffness;
        private float springDamping;
        private bool enableMotor;
        private bool enableMotorFreeSpin;
        private float motorForceLimit = float.MaxValue;
        private float motorGearRatio = 1;
        private float motorTargetVelocity = 0;

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

        [EditorProperty("Axis")]
        public Vector3 Axis
        {
            get => axis;
            set
            {
                axis = value;
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
        public float Angle
        {
            get
            {
                if (joint == null || updating)
                {
                    return 0.0f;
                }

                return joint->GetAngle();
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

        [EditorCategory("Motor")]
        [EditorProperty("Enable Motor")]
        public bool EnableMotor
        {
            get => enableMotor;
            set
            {
                enableMotor = value;

                if (joint != null && !updating)
                {
                    joint->SetRevoluteJointFlagMut(PxRevoluteJointFlag.DriveEnabled, value);
                }
            }
        }

        [EditorCategory("Motor")]
        [EditorProperty("Target Velocity")]
        public float MotorTargetVelocity
        {
            get => motorTargetVelocity;
            set
            {
                value = MathUtil.Clamp(value, float.MinValue, float.MaxValue);

                motorTargetVelocity = value;

                if (joint != null && !updating && !Application.InEditMode)
                {
                    joint->SetDriveVelocityMut(value, true);
                }
            }
        }

        [EditorCategory("Motor")]
        [EditorProperty("Free Spin")]
        public bool EnableMotorFreeSpin
        {
            get => enableMotorFreeSpin;
            set
            {
                enableMotorFreeSpin = value;

                if (joint != null && !updating)
                {
                    joint->SetRevoluteJointFlagMut(PxRevoluteJointFlag.DriveFreespin, value);
                }
            }
        }

        [EditorCategory("Motor")]
        [EditorProperty("Gear Ratio")]
        public float MotorGearRatio
        {
            get => motorGearRatio;
            set
            {
                motorGearRatio = value;

                if (joint != null && !updating)
                {
                    joint->SetDriveGearRatioMut(value);
                }
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
                    joint->SetRevoluteJointFlagMut(PxRevoluteJointFlag.LimitEnabled, value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Max", EditorPropertyMode.SliderAngle)]
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
        [EditorProperty("Min", EditorPropertyMode.SliderAngle)]
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

        [EditorCategory("Limits")]
        [EditorProperty("Motor Force Limit")]
        public float MotorForceLimit
        {
            get => motorForceLimit;
            set
            {
                value = MathUtil.Clamp(value, 0, float.MaxValue);

                motorForceLimit = value;

                if (joint != null && !updating)
                {
                    joint->SetDriveForceLimitMut(value);
                }
            }
        }

        public void SetMotorVelocity(float value, bool autoAwake = true)
        {
            if (joint == null || updating)
            {
                return;
            }

            value = MathUtil.Clamp(value, float.MinValue, float.MaxValue);

            joint->SetDriveVelocityMut(value, autoAwake);
        }

        protected override unsafe PxJoint* CreateJoint(PxPhysics* physics, PxScene* scene, RigidBody rigidBody0, RigidBody rigidBody1, Transform transform0, Transform transform1)
        {
            Vector3 connectedAnchor = AutoCalculate ? transform0.GlobalPosition + Anchor - transform1.GlobalPosition : ConnectedAnchor;

            // Calculate local transforms for the hinge joint
            Matrix4x4 localTransform0 = CalculateLocalTransform(Anchor, Axis);
            Matrix4x4 localTransform1 = CalculateLocalTransform(connectedAnchor, Axis);

            // Convert local transforms to PxTransform objects
            PxTransform local0 = Helper.Convert(localTransform0);
            PxTransform local1 = Helper.Convert(localTransform1);

            joint = physics->PhysPxRevoluteJointCreate(rigidBody0.Actor, &local0, rigidBody1.Actor, &local1);
            joint->SetRevoluteJointFlagMut(PxRevoluteJointFlag.DriveEnabled, enableMotor);
            joint->SetRevoluteJointFlagMut(PxRevoluteJointFlag.DriveFreespin, enableMotorFreeSpin);
            joint->SetDriveGearRatioMut(motorGearRatio);
            joint->SetRevoluteJointFlagMut(PxRevoluteJointFlag.LimitEnabled, enableLimit);
            UpdateLimits();
            joint->SetDriveForceLimitMut(motorForceLimit);

            if (!Application.InEditMode)
            {
                joint->SetDriveVelocityMut(motorTargetVelocity, true);
            }

            return (PxJoint*)joint;
        }

        private void UpdateLimits()
        {
            if (joint == null || updating)
            {
                return;
            }

            PxJointAngularLimitPair limits;
            limits.lower = limitMin;
            limits.upper = limitMax;
            limits.restitution = limitRestitution;
            limits.bounceThreshold = limitBounceThreshold;
            limits.stiffness = springStiffness;
            limits.damping = springDamping;

            joint->SetLimitMut(&limits);
        }

        // Calculate local transform for the hinge joint based on anchor point and axis
        private static Matrix4x4 CalculateLocalTransform(Vector3 anchor, Vector3 axis)
        {
            // Calculate rotation matrix to align the x-axis with the hinge axis
            Vector3 forward = Vector3.Normalize(axis);
            Vector3 right = Vector3.Cross(Vector3.UnitY, forward); // Assuming up direction is Vector3.UnitY
            Vector3 up = Vector3.Cross(forward, right);

            Matrix4x4 rotationMatrix = new(
                right.X, up.X, forward.X, 0,
                right.Y, up.Y, forward.Y, 0,
                right.Z, up.Z, forward.Z, 0,
                0, 0, 0, 1);

            // Set translation based on anchor point
            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(anchor);

            // Combine rotation and translation to get the local transform
            return rotationMatrix * translationMatrix;
        }
    }
}