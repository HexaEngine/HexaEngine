namespace HexaEngine.Physics.Joints
{
    using HexaEngine.Core;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Physics;
    using HexaEngine.Scenes.Serialization;
    using MagicPhysX;
    using System;
    using System.Numerics;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Physics.D6Joint")]
    [EditorCategory("Joints", "Physics")]
    [EditorComponent<D6Joint>("D6 Joint")]
    public sealed unsafe class D6Joint : Joint
    {
        private PxD6Joint* joint;
        private bool autoCalculate;
        private Vector3 anchor;
        private Vector3 axis = new(1, 0, 0);
        private Vector3 connectedAnchor;
        private Vector3 secondaryAxis = new(0, 1, 0);
        private D6Motion motionX = D6Motion.Free;
        private D6Motion motionY = D6Motion.Free;
        private D6Motion motionZ = D6Motion.Free;
        private D6Motion twist = D6Motion.Free;
        private D6Motion swingY = D6Motion.Free;
        private D6Motion swingZ = D6Motion.Free;
        private Vector3 linearSpringStiffness;
        private Vector3 linearSpringDamping;
        private Vector3 linearLimitMax = new(float.MaxValue);
        private Vector3 linearLimitMin = new(float.MinValue);
        private Vector3 linearLimitRestitution;
        private Vector3 linearLimitBounceThreshold;
        private float twistSpringStiffness;
        private float twistSpringDamping;
        private float twistLimitMax;
        private float twistLimitMin;
        private float twistLimitRestitution;
        private float twistLimitBounceThreshold;
        private float swingSpringStiffness;
        private float swingSpringDamping;
        private Vector2 swingLimitMax;
        private Vector2 swingLimitMin;
        private float swingLimitRestitution;
        private float swingLimitBounceThreshold;
        private Vector3 motorTargetPosition;
        private Quaternion motorTargetRotation;
        private Vector3 linearMotorTargetVelocity;
        private Vector3 linearMotorStiffness;
        private Vector3 linearMotorDamping;
        private Vector3 linearMotorForceLimit = new(float.MaxValue);
        private bool linearMotorIsAccelerationX;
        private bool linearMotorIsAccelerationY;
        private bool linearMotorIsAccelerationZ;
        private D6AngularMotorMode angularMotorMode;
        private float twistMotorTargetVelocity;
        private float twistMotorStiffness;
        private float twistMotorDamping;
        private float twistMotorForceLimit = float.MaxValue;
        private bool twistMotorIsAcceleration;
        private Vector2 swingMotorTargetVelocity;
        private float swingMotorStiffness;
        private float swingMotorDamping;
        private float swingMotorForceLimit = float.MaxValue;
        private bool swingMotorIsAcceleration;
        private Vector3 sLerpMotorTargetVelocity;
        private float sLerpMotorStiffness;
        private float sLerpMotorDamping;
        private float sLerpMotorForceLimit = float.MaxValue;
        private bool sLerpMotorIsAcceleration;
        private float projectionLinearTolerance;
        private float projectionAngularTolerance;

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

        [EditorProperty("Secondary Axis")]
        public Vector3 SecondaryAxis
        {
            get => secondaryAxis;
            set
            {
                secondaryAxis = value;
            }
        }

        [EditorProperty<D6Motion>("Motion X")]
        public D6Motion MotionX
        {
            get => motionX;
            set
            {
                motionX = value;
                if (joint != null && !updating)
                {
                    joint->SetMotionMut(PxD6Axis.X, Helper.Convert(value));
                }
            }
        }

        [EditorProperty<D6Motion>("Motion Y")]
        public D6Motion MotionY
        {
            get => motionY;
            set
            {
                motionY = value;
                if (joint != null && !updating)
                {
                    joint->SetMotionMut(PxD6Axis.Y, Helper.Convert(value));
                }
            }
        }

        [EditorProperty<D6Motion>("Motion Z")]
        public D6Motion MotionZ
        {
            get => motionZ;
            set
            {
                motionZ = value;
                if (joint != null && !updating)
                {
                    joint->SetMotionMut(PxD6Axis.Z, Helper.Convert(value));
                }
            }
        }

        [EditorProperty<D6Motion>("Twist")]
        public D6Motion Twist
        {
            get => twist;
            set
            {
                twist = value;
                if (joint != null && !updating)
                {
                    joint->SetMotionMut(PxD6Axis.Twist, Helper.Convert(value));
                }
            }
        }

        [EditorProperty<D6Motion>("Swing Y")]
        public D6Motion SwingY
        {
            get => swingY;
            set
            {
                swingY = value;
                if (joint != null && !updating)
                {
                    joint->SetMotionMut(PxD6Axis.Swing1, Helper.Convert(value));
                }
            }
        }

        [EditorProperty<D6Motion>("Swing Z")]
        public D6Motion SwingZ
        {
            get => swingZ;
            set
            {
                swingZ = value;
                if (joint != null && !updating)
                {
                    joint->SetMotionMut(PxD6Axis.Swing2, Helper.Convert(value));
                }
            }
        }

        [EditorCategory("Linear Spring")]
        [EditorProperty("Stiffness")]
        public Vector3 LinearSpringStiffness
        {
            get => linearSpringStiffness;
            set
            {
                var old = linearSpringStiffness;
                linearSpringStiffness = value;
                UpdateLinearLimits(value, old);
            }
        }

        [EditorCategory("Linear Spring")]
        [EditorProperty("Damping")]
        public Vector3 LinearSpringDamping
        {
            get => linearSpringDamping;
            set
            {
                var old = linearSpringDamping;
                linearSpringDamping = value;
                UpdateLinearLimits(value, old);
            }
        }

        [EditorCategory("Linear Limit")]
        [EditorProperty("Max")]
        public Vector3 LinearLimitMax
        {
            get => linearLimitMax;
            set
            {
                var old = linearLimitMax;
                linearLimitMax = value;
                UpdateLinearLimits(value, old);
            }
        }

        [EditorCategory("Linear Limit")]
        [EditorProperty("Min")]
        public Vector3 LinearLimitMin
        {
            get => linearLimitMin;
            set
            {
                var old = linearLimitMin;
                linearLimitMin = value;
                UpdateLinearLimits(value, old);
            }
        }

        [EditorCategory("Linear Limit")]
        [EditorProperty("Restitution")]
        public Vector3 LinearLimitRestitution
        {
            get => linearLimitRestitution;
            set
            {
                var old = linearLimitRestitution;
                linearLimitRestitution = value;
                UpdateLinearLimits(value, old);
            }
        }

        [EditorCategory("Linear Limit")]
        [EditorProperty("Bounce Threshold")]
        public Vector3 LinearLimitBounceThreshold
        {
            get => linearLimitBounceThreshold;
            set
            {
                var old = linearLimitBounceThreshold;
                linearLimitBounceThreshold = value;
                UpdateLinearLimits(value, old);
            }
        }

        [EditorCategory("Twist Spring")]
        [EditorProperty("Stiffness")]
        public float TwistSpringStiffness
        {
            get => twistSpringStiffness;
            set
            {
                twistSpringStiffness = value;
                UpdateAngularTwistLimits();
            }
        }

        [EditorCategory("Twist Spring")]
        [EditorProperty("Damping")]
        public float TwistSpringDamping
        {
            get => twistSpringDamping;
            set
            {
                twistSpringDamping = value;
                UpdateAngularTwistLimits();
            }
        }

        [EditorCategory("Twist Limit")]
        [EditorProperty("Max")]
        public float TwistLimitMax
        {
            get => twistLimitMax;
            set
            {
                twistLimitMax = value;
                UpdateAngularTwistLimits();
            }
        }

        [EditorCategory("Twist Limit")]
        [EditorProperty("Min")]
        public float TwistLimitMin
        {
            get => twistLimitMin;
            set
            {
                twistLimitMin = value;
                UpdateAngularTwistLimits();
            }
        }

        [EditorCategory("Twist Limit")]
        [EditorProperty("Restitution")]
        public float TwistLimitRestitution
        {
            get => twistLimitRestitution;
            set
            {
                twistLimitRestitution = value;
                UpdateAngularTwistLimits();
            }
        }

        [EditorCategory("Twist Limit")]
        [EditorProperty("Bounce Threshold")]
        public float TwistLimitBounceThreshold
        {
            get => twistLimitBounceThreshold;
            set
            {
                twistLimitBounceThreshold = value;
                UpdateAngularTwistLimits();
            }
        }

        [EditorCategory("Swing Spring")]
        [EditorProperty("Stiffness")]
        public float SwingSpringStiffness
        {
            get => swingSpringStiffness;
            set
            {
                swingSpringStiffness = value;
                UpdateAngularSwingLimits();
            }
        }

        [EditorCategory("Swing Spring")]
        [EditorProperty("Damping")]
        public float SwingSpringDamping
        {
            get => swingSpringDamping;
            set
            {
                swingSpringDamping = value;
                UpdateAngularSwingLimits();
            }
        }

        [EditorCategory("Swing Limit")]
        [EditorProperty("Max")]
        public Vector2 SwingLimitMax
        {
            get => swingLimitMax;
            set
            {
                swingLimitMax = value;
                UpdateAngularSwingLimits();
            }
        }

        [EditorCategory("Swing Limit")]
        [EditorProperty("Min")]
        public Vector2 SwingLimitMin
        {
            get => swingLimitMin;
            set
            {
                swingLimitMin = value;
                UpdateAngularSwingLimits();
            }
        }

        [EditorCategory("Swing Limit")]
        [EditorProperty("Restitution")]
        public float SwingLimitRestitution
        {
            get => swingLimitRestitution;
            set
            {
                swingLimitRestitution = value;
                UpdateAngularSwingLimits();
            }
        }

        [EditorCategory("Swing Limit")]
        [EditorProperty("Bounce Threshold")]
        public float SwingLimitBounceThreshold
        {
            get => swingLimitBounceThreshold;
            set
            {
                swingLimitBounceThreshold = value;
                UpdateAngularSwingLimits();
            }
        }

        [EditorCategory("Motor")]
        [EditorProperty("Target Position")]
        public Vector3 MotorTargetPosition
        {
            get => motorTargetPosition;
            set
            {
                motorTargetPosition = value;
                UpdateMotorPosition();
            }
        }

        [EditorCategory("Motor")]
        [EditorProperty("Target Rotation")]
        public Quaternion MotorTargetRotation
        {
            get => motorTargetRotation;
            set
            {
                motorTargetRotation = value;
            }
        }

        [EditorCategory("Linear Motor")]
        [EditorProperty("Target Velocity")]
        public Vector3 LinearMotorTargetVelocity
        {
            get => linearMotorTargetVelocity;
            set
            {
                linearMotorTargetVelocity = value;
                UpdateMotorVelocity();
            }
        }

        [EditorCategory("Linear Motor")]
        [EditorProperty("Stiffness")]
        public Vector3 LinearMotorStiffness
        {
            get => linearMotorStiffness;
            set
            {
                linearMotorStiffness = value;
                UpdateLinearMotor();
            }
        }

        [EditorCategory("Linear Motor")]
        [EditorProperty("Damping")]
        public Vector3 LinearMotorDamping
        {
            get => linearMotorDamping;
            set
            {
                linearMotorDamping = value;
                UpdateLinearMotor();
            }
        }

        [EditorCategory("Linear Motor")]
        [EditorProperty("Force Limit")]
        public Vector3 LinearMotorForceLimit
        {
            get => linearMotorForceLimit;
            set
            {
                linearMotorForceLimit = value;
                UpdateLinearMotor();
            }
        }

        [EditorCategory("Linear Motor")]
        [EditorProperty("Is Acceleration X")]
        public bool LinearMotorIsAccelerationX
        {
            get => linearMotorIsAccelerationX;
            set
            {
                linearMotorIsAccelerationX = value;
                UpdateLinearMotor();
            }
        }

        [EditorCategory("Linear Motor")]
        [EditorProperty("Is Acceleration Y")]
        public bool LinearMotorIsAccelerationY
        {
            get => linearMotorIsAccelerationY;
            set
            {
                linearMotorIsAccelerationY = value;
                UpdateLinearMotor();
            }
        }

        [EditorCategory("Linear Motor")]
        [EditorProperty("Is Acceleration Z")]
        public bool LinearMotorIsAccelerationZ
        {
            get => linearMotorIsAccelerationZ;
            set
            {
                linearMotorIsAccelerationZ = value;
                UpdateLinearMotor();
            }
        }

        [EditorProperty<D6AngularMotorMode>("Angular Motor Mode")]
        public D6AngularMotorMode AngularMotorMode
        {
            get => angularMotorMode;
            set
            {
                angularMotorMode = value;
                UpdateAngularMotor();
            }
        }

        private static bool IsTwistAndSwingMotor(D6Joint joint)
        {
            return joint.angularMotorMode == D6AngularMotorMode.TwistAndSwing;
        }

        private static bool IsSLerpMotor(D6Joint joint)
        {
            return joint.angularMotorMode == D6AngularMotorMode.SLerp;
        }

        [EditorCategory("Twist Motor")]
        [EditorProperty("Target Velocity")]
        [EditorPropertyCondition<D6Joint>(nameof(IsTwistAndSwingMotor))]
        public float TwistMotorTargetVelocity
        {
            get => twistMotorTargetVelocity;
            set
            {
                twistMotorTargetVelocity = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("Twist Motor")]
        [EditorProperty("Stiffness")]
        [EditorPropertyCondition<D6Joint>(nameof(IsTwistAndSwingMotor))]
        public float TwistMotorStiffness
        {
            get => twistMotorStiffness;
            set
            {
                twistMotorStiffness = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("Twist Motor")]
        [EditorProperty("Damping")]
        [EditorPropertyCondition<D6Joint>(nameof(IsTwistAndSwingMotor))]
        public float TwistMotorDamping
        {
            get => twistMotorDamping;
            set
            {
                twistMotorDamping = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("Twist Motor")]
        [EditorProperty("Force Limit")]
        [EditorPropertyCondition<D6Joint>(nameof(IsTwistAndSwingMotor))]
        public float TwistMotorForceLimit
        {
            get => twistMotorForceLimit;
            set
            {
                twistMotorForceLimit = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("Twist Motor")]
        [EditorProperty("Is Acceleration")]
        [EditorPropertyCondition<D6Joint>(nameof(IsTwistAndSwingMotor))]
        public bool TwistMotorIsAcceleration
        {
            get => twistMotorIsAcceleration;
            set
            {
                twistMotorIsAcceleration = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("Swing Motor")]
        [EditorProperty("Target Velocity")]
        [EditorPropertyCondition<D6Joint>(nameof(IsTwistAndSwingMotor))]
        public Vector2 SwingMotorTargetVelocity
        {
            get => swingMotorTargetVelocity;
            set
            {
                swingMotorTargetVelocity = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("Swing Motor")]
        [EditorProperty("Stiffness")]
        [EditorPropertyCondition<D6Joint>(nameof(IsTwistAndSwingMotor))]
        public float SwingMotorStiffness
        {
            get => swingMotorStiffness;
            set
            {
                swingMotorStiffness = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("Swing Motor")]
        [EditorProperty("Damping")]
        [EditorPropertyCondition<D6Joint>(nameof(IsTwistAndSwingMotor))]
        public float SwingMotorDamping
        {
            get => swingMotorDamping;
            set
            {
                swingMotorDamping = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("Swing Motor")]
        [EditorProperty("Force Limit")]
        [EditorPropertyCondition<D6Joint>(nameof(IsTwistAndSwingMotor))]
        public float SwingMotorForceLimit
        {
            get => swingMotorForceLimit;
            set
            {
                swingMotorForceLimit = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("Swing Motor")]
        [EditorProperty("Is Acceleration")]
        [EditorPropertyCondition<D6Joint>(nameof(IsTwistAndSwingMotor))]
        public bool SwingMotorIsAcceleration
        {
            get => swingMotorIsAcceleration;
            set
            {
                swingMotorIsAcceleration = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("SLerp Motor")]
        [EditorProperty("Target Velocity")]
        [EditorPropertyCondition<D6Joint>(nameof(IsSLerpMotor))]
        public Vector3 SLerpMotorTargetVelocity
        {
            get => sLerpMotorTargetVelocity;
            set
            {
                sLerpMotorTargetVelocity = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("SLerp Motor")]
        [EditorProperty("Stiffness")]
        [EditorPropertyCondition<D6Joint>(nameof(IsSLerpMotor))]
        public float SLerpMotorStiffness
        {
            get => sLerpMotorStiffness;
            set
            {
                sLerpMotorStiffness = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("SLerp Motor")]
        [EditorProperty("Damping")]
        [EditorPropertyCondition<D6Joint>(nameof(IsSLerpMotor))]
        public float SLerpMotorDamping
        {
            get => sLerpMotorDamping;
            set
            {
                sLerpMotorDamping = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("SLerp Motor")]
        [EditorProperty("Force Limit")]
        [EditorPropertyCondition<D6Joint>(nameof(IsSLerpMotor))]
        public float SLerpMotorForceLimit
        {
            get => sLerpMotorForceLimit;
            set
            {
                sLerpMotorForceLimit = value;
                UpdateAngularMotor();
            }
        }

        [EditorCategory("SLerp Motor")]
        [EditorProperty("Is Acceleration")]
        [EditorPropertyCondition<D6Joint>(nameof(IsSLerpMotor))]
        public bool SLerpMotorIsAcceleration
        {
            get => sLerpMotorIsAcceleration;
            set
            {
                sLerpMotorIsAcceleration = value;
                UpdateAngularMotor();
            }
        }

        [EditorProperty("Projection Linear Tolerance")]
        public float ProjectionLinearTolerance
        {
            get => projectionLinearTolerance;
            set
            {
                projectionLinearTolerance = value;
                if (joint != null && !updating)
                {
                    joint->SetProjectionLinearToleranceMut(value);
                }
            }
        }

        [EditorProperty("Projection Angular Tolerance")]
        public float ProjectionAngularTolerance
        {
            get => projectionAngularTolerance;
            set
            {
                projectionAngularTolerance = value;
                if (joint != null && !updating)
                {
                    joint->SetProjectionAngularToleranceMut(value);
                }
            }
        }

        protected override unsafe PxJoint* CreateJoint(PxPhysics* physics, PxScene* scene, RigidBody rigidBody0, RigidBody rigidBody1, Transform transform0, Transform transform1)
        {
            Vector3 connectedAnchor = AutoCalculate ? transform0.GlobalPosition + Anchor - transform1.GlobalPosition : ConnectedAnchor;

            // Calculate local transforms for the hinge joint
            Matrix4x4 localTransform0 = CalculateLocalTransform(Anchor, Axis);
            Matrix4x4 localTransform1 = CalculateLocalTransform(connectedAnchor, SecondaryAxis);

            // Convert local transforms to PxTransform objects
            PxTransform local0 = Helper.Convert(localTransform0);
            PxTransform local1 = Helper.Convert(localTransform1);

            joint = physics->PhysPxD6JointCreate(rigidBody0.Actor, &local0, rigidBody1.Actor, &local1);

            joint->SetMotionMut(PxD6Axis.X, Helper.Convert(motionX));
            joint->SetMotionMut(PxD6Axis.Y, Helper.Convert(motionY));
            joint->SetMotionMut(PxD6Axis.Z, Helper.Convert(motionZ));
            joint->SetMotionMut(PxD6Axis.Twist, Helper.Convert(twist));
            joint->SetMotionMut(PxD6Axis.Swing1, Helper.Convert(swingY));
            joint->SetMotionMut(PxD6Axis.Swing2, Helper.Convert(swingZ));

            ConfigureJoint();

            if (!Application.InEditMode)
            {
                //joint->SetDriveVelocityMut(motorVelocity, true);
            }

            return (PxJoint*)joint;
        }

        private void UpdateLinearLimits(Vector3 newValue, Vector3 oldValue)
        {
            if (joint == null || updating)
            {
                return;
            }

            if (newValue.ComponentsEquals() &&
                linearLimitMin.ComponentsEquals() &&
                linearLimitMax.ComponentsEquals() &&
                linearLimitMin == -linearLimitMax &&
                linearLimitRestitution.ComponentsEquals() &&
                linearLimitBounceThreshold.ComponentsEquals() &&
                linearSpringStiffness.ComponentsEquals() &&
                linearSpringDamping.ComponentsEquals())
            {
                PxJointLinearLimit limit;
                limit.value = linearLimitMax.X;
                limit.restitution = linearLimitRestitution.X;
                limit.bounceThreshold = linearLimitBounceThreshold.X;
                limit.stiffness = linearSpringStiffness.X;
                limit.damping = linearSpringDamping.X;
                joint->SetDistanceLimitMut(&limit);
                return;
            }

            if (newValue.X != oldValue.X)
            {
                PxJointLinearLimitPair limit;
                limit.upper = linearLimitMax.X;
                limit.lower = linearLimitMin.X;
                limit.restitution = linearLimitRestitution.X;
                limit.bounceThreshold = linearLimitBounceThreshold.X;
                limit.stiffness = linearSpringStiffness.X;
                limit.damping = linearSpringDamping.X;
                joint->SetLinearLimitMut(PxD6Axis.X, &limit);
            }

            if (newValue.Y != oldValue.Y)
            {
                PxJointLinearLimitPair limit;
                limit.upper = linearLimitMax.Y;
                limit.lower = linearLimitMin.Y;
                limit.restitution = linearLimitRestitution.Y;
                limit.bounceThreshold = linearLimitBounceThreshold.Y;
                limit.stiffness = linearSpringStiffness.Y;
                limit.damping = linearSpringDamping.Y;
                joint->SetLinearLimitMut(PxD6Axis.Y, &limit);
            }

            if (newValue.Z != oldValue.Z)
            {
                PxJointLinearLimitPair limit;
                limit.upper = linearLimitMax.Z;
                limit.lower = linearLimitMin.Z;
                limit.restitution = linearLimitRestitution.Z;
                limit.bounceThreshold = linearLimitBounceThreshold.Z;
                limit.stiffness = linearSpringStiffness.Z;
                limit.damping = linearSpringDamping.Z;
                joint->SetLinearLimitMut(PxD6Axis.Z, &limit);
            }
        }

        private void UpdateAngularTwistLimits()
        {
            if (joint == null || updating)
            {
                return;
            }

            PxJointAngularLimitPair limit;
            limit.upper = twistLimitMax;
            limit.lower = twistLimitMin;
            limit.restitution = twistLimitRestitution;
            limit.bounceThreshold = twistLimitBounceThreshold;
            limit.stiffness = twistSpringStiffness;
            limit.damping = twistSpringDamping;

            joint->SetTwistLimitMut(&limit);
        }

        private void UpdateAngularSwingLimits()
        {
            if (joint == null || updating)
            {
                return;
            }

            if (swingLimitMax.ComponentsEquals() &&
                swingLimitMin.ComponentsEquals() &&
                swingLimitMin == -swingLimitMax)
            {
                PxJointLimitCone limit;
                limit.yAngle = swingLimitMax.X;
                limit.zAngle = swingLimitMax.Y;
                limit.restitution = swingLimitRestitution;
                limit.bounceThreshold = swingLimitBounceThreshold;
                limit.stiffness = swingSpringStiffness;
                limit.damping = swingSpringDamping;

                joint->SetSwingLimitMut(&limit);
            }
            else
            {
                PxJointLimitPyramid limit;
                limit.yAngleMin = swingLimitMin.X;
                limit.yAngleMax = swingLimitMax.X;
                limit.zAngleMax = swingLimitMax.Y;
                limit.zAngleMin = swingLimitMin.Y;
                limit.restitution = swingLimitRestitution;
                limit.bounceThreshold = swingLimitBounceThreshold;
                limit.stiffness = swingSpringStiffness;
                limit.damping = swingSpringDamping;

                joint->SetPyramidSwingLimitMut(&limit);
            }
        }

        private void UpdateLinearMotor()
        {
            {
                PxD6JointDrive drive;
                drive.stiffness = linearMotorStiffness.X;
                drive.damping = linearMotorDamping.X;
                drive.forceLimit = linearMotorForceLimit.X;
                drive.flags = linearMotorIsAccelerationX ? PxD6JointDriveFlags.Acceleration : 0;
                joint->SetDriveMut(PxD6Drive.X, &drive);
            }

            {
                PxD6JointDrive drive;
                drive.stiffness = linearMotorStiffness.Y;
                drive.damping = linearMotorDamping.Y;
                drive.forceLimit = linearMotorForceLimit.Y;
                drive.flags = linearMotorIsAccelerationY ? PxD6JointDriveFlags.Acceleration : 0;
                joint->SetDriveMut(PxD6Drive.Y, &drive);
            }

            {
                PxD6JointDrive drive;
                drive.stiffness = linearMotorStiffness.Z;
                drive.damping = linearMotorDamping.Z;
                drive.forceLimit = linearMotorForceLimit.Z;
                drive.flags = linearMotorIsAccelerationZ ? PxD6JointDriveFlags.Acceleration : 0;
                joint->SetDriveMut(PxD6Drive.Z, &drive);
            }
        }

        private void UpdateAngularMotor()
        {
            if (angularMotorMode == D6AngularMotorMode.TwistAndSwing)
            {
                {
                    PxD6JointDrive drive;
                    drive.stiffness = twistMotorStiffness;
                    drive.damping = twistMotorDamping;
                    drive.forceLimit = twistMotorForceLimit;
                    drive.flags = twistMotorIsAcceleration ? PxD6JointDriveFlags.Acceleration : 0;
                    joint->SetDriveMut(PxD6Drive.Twist, &drive);
                }

                {
                    PxD6JointDrive drive;
                    drive.stiffness = swingMotorStiffness;
                    drive.damping = swingMotorDamping;
                    drive.forceLimit = swingMotorForceLimit;
                    drive.flags = swingMotorIsAcceleration ? PxD6JointDriveFlags.Acceleration : 0;
                    joint->SetDriveMut(PxD6Drive.Swing, &drive);
                }
            }
            else if (angularMotorMode == D6AngularMotorMode.SLerp)
            {
                PxD6JointDrive drive;
                drive.stiffness = sLerpMotorStiffness;
                drive.damping = sLerpMotorDamping;
                drive.forceLimit = sLerpMotorForceLimit;
                drive.flags = sLerpMotorIsAcceleration ? PxD6JointDriveFlags.Acceleration : 0;
                joint->SetDriveMut(PxD6Drive.Slerp, &drive);
            }
            else
            {
                throw new NotSupportedException($"D6AngularMotorMode ({angularMotorMode}) is not supported!");
            }
        }

        private void UpdateMotorVelocity()
        {
            if (joint == null || updating || Application.InEditMode)
            {
                return;
            }

            Vector3 linear = linearMotorTargetVelocity;
            Vector3 angular = angularMotorMode == D6AngularMotorMode.TwistAndSwing ? new(twistMotorTargetVelocity, swingMotorTargetVelocity.X, swingMotorTargetVelocity.Y) : sLerpMotorTargetVelocity;
            joint->SetDriveVelocityMut((PxVec3*)&linear, (PxVec3*)&angular, true);
        }

        private void UpdateMotorPosition()
        {
            if (joint == null || updating || Application.InEditMode)
            {
                return;
            }

            PxTransform transform = Helper.Convert(motorTargetPosition, motorTargetRotation);
            joint->SetDrivePositionMut(&transform, true);
        }

        private void ConfigureJoint()
        {
            if (joint == null || updating)
            {
                return;
            }

            UpdateLinearLimits(Vector3.One, default);
            UpdateAngularTwistLimits();
            UpdateAngularSwingLimits();
            UpdateLinearMotor();
            UpdateAngularMotor();
            UpdateMotorVelocity();
            UpdateMotorPosition();
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