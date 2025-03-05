namespace HexaEngine.Physics
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes.Serialization;
    using MagicPhysX;
    using System.Collections.Generic;
    using System.Numerics;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Physics.RigidBody")]
    [EditorCategory("Physics")]
    [EditorComponent<RigidBody>("Rigid Body", Icon = "\xf192")]
    public unsafe class RigidBody : Actor, IRigidBodyComponent
    {
        protected PxRigidActor* actor;
        protected PxTransform pose;
        private List<IColliderComponent>? colliders;

        private PxRigidBodyFlags bodyFlags;

        private float sleepThreshold = 5e-5f * PhysicsSystem.TolerancesScale.speed * PhysicsSystem.TolerancesScale.speed;
        private float stabilizationThreshold = 1e-5f * PhysicsSystem.TolerancesScale.speed * PhysicsSystem.TolerancesScale.speed;
        private float linearDamping = 0;
        private float angularDamping = 0.05f;
        private float maxLinearVelocity = 1e+16f;
        private float maxAngularVelocity = 100;
        private float contactReportThreshold = float.MaxValue;
        private float minCCDAdvanceCoefficient = 0.15f;
        private float maxDepenetrationVelocity = float.MaxValue;
        private float maxContactImpulse = float.MaxValue;
        private float contactSlopCoefficient = 0;

        public RigidBody()
        {
        }

        internal RigidBody(PxRigidDynamic* actor, ActorType actorType, CharacterController characterController)
        {
            this.actor = (PxRigidActor*)actor;
            type = actorType;
            CharacterController = characterController;
        }

        internal PxRigidActor* Actor => actor;

        protected override void Awake()
        {
            base.Awake();
            GameObject.Transform.FlagsChanged += GameObjectTransformFlagsChanged;
        }

        protected override void Destroy()
        {
            GameObject.Transform.FlagsChanged -= GameObjectTransformFlagsChanged;
            base.Destroy();
        }

        private void GameObjectTransformFlagsChanged(Transform transform, TransformFlags flags)
        {
            if (actor != null && type != ActorType.Static)
            {
                ((PxRigidDynamic*)actor)->SetRigidDynamicLockFlagsMut(Helper.Convert(flags));
            }
        }

        protected override PxActor* CreateActor(PxPhysics* physics, PxScene* scene)
        {
            DestroyActor();

            colliders = GameObject.DiscoverComponents<IColliderComponent, RigidBody>().ToList();

            PxTransform transform = Helper.Convert(GameObject.Transform.GlobalPosition, GameObject.Transform.GlobalOrientation);

            if (type == ActorType.Kinematic)
            {
                bodyFlags |= PxRigidBodyFlags.Kinematic;
            }
            else
            {
                bodyFlags &= ~PxRigidBodyFlags.Kinematic;
            }

            switch (type)
            {
                case ActorType.Static:
                    var rigidStatic = physics->CreateRigidStaticMut(&transform);
                    actor = (PxRigidActor*)rigidStatic;
                    break;

                case ActorType.Dynamic:
                case ActorType.Kinematic:
                    var rigidDynamic = physics->CreateRigidDynamicMut(&transform);
                    actor = (PxRigidActor*)rigidDynamic;
                    rigidDynamic->SetSleepThresholdMut(sleepThreshold);
                    rigidDynamic->SetStabilizationThresholdMut(stabilizationThreshold);
                    rigidDynamic->SetContactReportThresholdMut(contactReportThreshold);
                    rigidDynamic->SetRigidDynamicLockFlagsMut(Helper.Convert(GameObject.Transform.Flags));
                    var rigidBody = (PxRigidBody*)rigidDynamic;
                    rigidBody->SetRigidBodyFlagsMut(bodyFlags);
                    rigidBody->SetLinearDampingMut(linearDamping);
                    rigidBody->SetAngularDampingMut(angularDamping);
                    rigidBody->SetMaxLinearVelocityMut(maxLinearVelocity);
                    rigidBody->SetMaxAngularVelocityMut(maxAngularVelocity);
                    rigidBody->SetMinCCDAdvanceCoefficientMut(minCCDAdvanceCoefficient);
                    rigidBody->SetMaxDepenetrationVelocityMut(maxDepenetrationVelocity);
                    rigidBody->SetMaxContactImpulseMut(maxContactImpulse);
                    rigidBody->SetContactSlopCoefficientMut(contactSlopCoefficient);
                    break;
            }

            if (actor == null)
            {
                PhysicsSystem.Logger.Error($"{GameObject.FullName}: Couldn't create actor");
                return null;
            }

            for (int i = 0; i < colliders.Count; i++)
            {
                IColliderComponent collider = colliders[i];
                collider.AddShapes(physics, scene, actor, GameObject.Transform);
            }

            return (PxActor*)actor;
        }

        protected override void DestroyActor()
        {
            if (colliders != null)
            {
                for (int i = 0; i < colliders.Count; i++)
                {
                    colliders[i].DestroyShapes();
                }
                colliders = null;
            }

            if (actor != null)
            {
                actor = null;
            }

            base.DestroyActor();
        }

        protected override void BeginUpdate()
        {
            if (type != ActorType.Static && GameObject != null)
            {
                //PxTransform transform = Helper.Convert(GameObject.Transform.PositionRotation);
                //actor->SetGlobalPoseMut(&transform, true);
            }
        }

        protected override void EndUpdate()
        {
            if (type != ActorType.Static && GameObject != null)
            {
                var pose = actor->GetGlobalPose();
                GameObject.Transform.PositionRotation = Helper.Convert(pose);
            }
        }

        [JsonIgnore]
        public bool IsSleeping
        {
            get
            {
                if (type == ActorType.Static || actor == null || isUpdating)
                {
                    return true;
                }

                return ((PxRigidDynamic*)actor)->IsSleeping();
            }
        }

        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Sleep Threshold")]
        public float SleepThreshold
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return 0;
                }

                if (actor != null && !isUpdating)
                {
                    sleepThreshold = ((PxRigidDynamic*)actor)->GetSleepThreshold();
                }

                return sleepThreshold;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                sleepThreshold = MathUtil.Clamp(value, 0, float.MaxValue);

                if (actor != null && !isUpdating)
                {
                    ((PxRigidDynamic*)actor)->SetSleepThresholdMut(value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Stabilization Threshold")]
        public float StabilizationThreshold
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return 0;
                }

                if (actor != null && !isUpdating)
                {
                    stabilizationThreshold = ((PxRigidDynamic*)actor)->GetStabilizationThreshold();
                }

                return stabilizationThreshold;
            }
            set
            {
                if (type != ActorType.Dynamic)
                {
                    return;
                }

                stabilizationThreshold = MathUtil.Clamp(value, 0, float.PositiveInfinity);

                if (actor != null && !isUpdating)
                {
                    ((PxRigidDynamic*)actor)->SetStabilizationThresholdMut(value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Linear Damping")]
        public float LinearDamping
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return 0;
                }

                if (actor != null && !isUpdating)
                {
                    linearDamping = ((PxRigidBody*)actor)->GetLinearDamping();
                }

                return linearDamping;
            }
            set
            {
                if (type != ActorType.Dynamic)
                {
                    return;
                }

                linearDamping = MathUtil.Clamp(value, 0, float.MaxValue);

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetLinearDampingMut(value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Angular Damping")]
        public float AngularDamping
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return 0;
                }

                if (actor != null && !isUpdating)
                {
                    angularDamping = ((PxRigidBody*)actor)->GetAngularDamping();
                }

                return angularDamping;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                angularDamping = MathUtil.Clamp(value, 0, float.MaxValue);

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetAngularDampingMut(value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Max Linear Velocity")]
        public float MaxLinearVelocity
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return 0;
                }

                if (actor != null && !isUpdating)
                {
                    maxLinearVelocity = ((PxRigidBody*)actor)->GetMaxLinearVelocity();
                }

                return maxLinearVelocity;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                maxLinearVelocity = MathUtil.Clamp(value, 0, 1e+16f);

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetMaxLinearVelocityMut(value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Max Angular Velocity")]
        public float MaxAngularVelocity
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return 0;
                }

                if (actor != null && !isUpdating)
                {
                    maxAngularVelocity = ((PxRigidBody*)actor)->GetMaxAngularVelocity();
                }

                return maxAngularVelocity;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                maxAngularVelocity = MathUtil.Clamp(value, 0, 1e+16f);

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetMaxAngularVelocityMut(value);
                }
            }
        }

        [EditorCategory("Contacts")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Contact Report Threshold")]
        public float ContactReportThreshold
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return 0;
                }

                if (actor != null && !isUpdating)
                {
                    contactReportThreshold = ((PxRigidDynamic*)actor)->GetContactReportThreshold();
                }

                return contactReportThreshold;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                contactReportThreshold = MathUtil.Clamp(value, 0, float.MaxValue);

                if (actor != null && !isUpdating)
                {
                    ((PxRigidDynamic*)actor)->SetContactReportThresholdMut(value);
                }
            }
        }

        [JsonIgnore]
        public Vector3 AngularVelocity
        {
            get
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return Vector3.Zero;
                }

                return ((PxRigidDynamic*)actor)->GetAngularVelocity();
            }

            set
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return;
                }

                ((PxRigidDynamic*)actor)->SetAngularVelocityMut((PxVec3*)&value, true);
            }
        }

        [JsonIgnore]
        public Vector3 LinearVelocity
        {
            get
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return Vector3.Zero;
                }

                return ((PxRigidDynamic*)actor)->GetLinearVelocity();
            }

            set
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return;
                }

                ((PxRigidDynamic*)actor)->SetLinearVelocityMut((PxVec3*)&value, true);
            }
        }

        [JsonIgnore]
        public float WakeCounter
        {
            get
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return 0;
                }

                return ((PxRigidDynamic*)actor)->GetWakeCounter();
            }
            set
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return;
                }

                value = MathUtil.Clamp(value, 0, float.MaxValue);

                ((PxRigidDynamic*)actor)->SetWakeCounterMut(value);
            }
        }

        [JsonIgnore]
        public float Mass
        {
            get
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return 0;
                }

                return ((PxRigidBody*)actor)->GetMass();
            }
            set
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return;
                }

                ((PxRigidBody*)actor)->SetMassMut(value);
            }
        }

        [JsonIgnore]
        public float InvMass
        {
            get
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return 0;
                }

                return ((PxRigidBody*)actor)->GetInvMass();
            }
        }

        [JsonIgnore]
        public Vector3 MassSpaceInertiaTensor
        {
            get
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return default;
                }

                return ((PxRigidBody*)actor)->GetMassSpaceInertiaTensor();
            }
            set
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return;
                }

                ((PxRigidBody*)actor)->SetMassSpaceInertiaTensorMut((PxVec3*)&value);
            }
        }

        [JsonIgnore]
        public Vector3 MassSpaceInvInertiaTensor
        {
            get
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return default;
                }

                return ((PxRigidBody*)actor)->GetMassSpaceInvInertiaTensor();
            }
        }

        [JsonIgnore]
        [EditorCategory("CCD")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Enable CCD")]
        public bool EnableCCD
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return default;
                }

                if (actor != null && !isUpdating)
                {
                    bodyFlags = ((PxRigidBody*)actor)->GetRigidBodyFlags();
                }

                return (bodyFlags & PxRigidBodyFlags.EnableCcd) != 0;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                bodyFlags |= PxRigidBodyFlags.EnableCcd;

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetRigidBodyFlagsMut(bodyFlags);
                }
            }
        }

        [JsonIgnore]
        [EditorCategory("CCD")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Enable CCD Friction")]
        public bool EnableCCDFriction
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return default;
                }

                if (actor != null && !isUpdating)
                {
                    bodyFlags = ((PxRigidBody*)actor)->GetRigidBodyFlags();
                }

                return (bodyFlags & PxRigidBodyFlags.EnableCcdFriction) != 0;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                bodyFlags |= PxRigidBodyFlags.EnableCcdFriction;

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetRigidBodyFlagsMut(bodyFlags);
                }
            }
        }

        [JsonIgnore]
        [EditorCategory("CCD")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Enable CCD Max Contact Impulse")]
        public bool EnableCCDMaxContactImpulse
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return default;
                }

                if (actor != null && !isUpdating)
                {
                    bodyFlags = ((PxRigidBody*)actor)->GetRigidBodyFlags();
                }

                return (bodyFlags & PxRigidBodyFlags.EnableCcdMaxContactImpulse) != 0;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                bodyFlags |= PxRigidBodyFlags.EnableCcdMaxContactImpulse;

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetRigidBodyFlagsMut(bodyFlags);
                }
            }
        }

        [JsonIgnore]
        [EditorCategory("CCD")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Enable Speculative CCD")]
        public bool EnableSpeculativeCCD
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return default;
                }

                if (actor != null && !isUpdating)
                {
                    bodyFlags = ((PxRigidBody*)actor)->GetRigidBodyFlags();
                }

                return (bodyFlags & PxRigidBodyFlags.EnableSpeculativeCcd) != 0;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                bodyFlags |= PxRigidBodyFlags.EnableSpeculativeCcd;

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetRigidBodyFlagsMut(bodyFlags);
                }
            }
        }

        [EditorCategory("CCD")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Min CCD Advance Coefficient")]
        public float MinCCDAdvanceCoefficient
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return 0;
                }

                if (actor != null && !isUpdating)
                {
                    minCCDAdvanceCoefficient = ((PxRigidBody*)actor)->GetMinCCDAdvanceCoefficient();
                }

                return minCCDAdvanceCoefficient;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                minCCDAdvanceCoefficient = MathUtil.Clamp01(value);

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetMinCCDAdvanceCoefficientMut(value);
                }
            }
        }

        [EditorCategory("Contacts")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Max Depenetration Velocity")]
        public float MaxDepenetrationVelocity
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return 0;
                }

                if (actor != null && !isUpdating)
                {
                    maxDepenetrationVelocity = ((PxRigidBody*)actor)->GetMaxDepenetrationVelocity();
                }

                return maxDepenetrationVelocity;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                maxDepenetrationVelocity = MathUtil.Clamp(value, 0, float.MaxValue);

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetMaxDepenetrationVelocityMut(value);
                }
            }
        }

        [EditorCategory("Contacts")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Max Contact Impulse")]
        public float MaxContactImpulse
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return 0;
                }

                if (actor != null && !isUpdating)
                {
                    maxContactImpulse = ((PxRigidBody*)actor)->GetMaxContactImpulse();
                }

                return maxContactImpulse;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                maxContactImpulse = MathUtil.Clamp(value, 0, float.MaxValue);

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetMaxContactImpulseMut(value);
                }
            }
        }

        [EditorCategory("Contacts")]
        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Contact Slop Coefficient")]
        public float ContactSlopCoefficient
        {
            get
            {
                if (type == ActorType.Static)
                {
                    return 0;
                }

                if (actor != null && !isUpdating)
                {
                    contactSlopCoefficient = ((PxRigidBody*)actor)->GetContactSlopCoefficient();
                }

                return contactSlopCoefficient;
            }
            set
            {
                if (type == ActorType.Static)
                {
                    return;
                }

                contactSlopCoefficient = MathUtil.Clamp(value, 0, float.MaxValue);

                if (actor != null && !isUpdating)
                {
                    ((PxRigidBody*)actor)->SetContactSlopCoefficientMut(value);
                }
            }
        }

        [JsonIgnore]
        public PxNodeIndex InternalIslandNodeIndex
        {
            get
            {
                if (type == ActorType.Static || isUpdating || actor == null)
                {
                    return default;
                }

                return ((PxRigidBody*)actor)->GetInternalIslandNodeIndex();
            }
        }

        [JsonIgnore]
        public IReadOnlyList<IColliderComponent> Colliders => colliders ?? throw new InvalidOperationException("Actor was not initialized!");

        /// <summary>
        /// Gets the <see cref="Physics.CharacterController"/> associated with the <see cref="RigidBody"/>.
        /// </summary>
        /// <remarks>Returns null if no <see cref="Physics.CharacterController"/> is associated.</remarks>
        [JsonIgnore]
        public CharacterController? CharacterController { get; internal set; }

        public event Action? OnWakeUp;

        public event Action? OnSleep;

        public void WakeUp()
        {
            if (type != ActorType.Dynamic)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot wake up static/kinematic actors.");
            }

            ((PxRigidDynamic*)actor)->WakeUpMut();
        }

        public void PutToSleep()
        {
            if (type != ActorType.Dynamic)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot put static/kinematic actors to sleep.");
            }

            ((PxRigidDynamic*)actor)->PutToSleepMut();
        }

        public void AddForce(Vector3 force, ForceMode mode, bool autoAwake = true)
        {
            if (type == ActorType.Static)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot add force for static actors.");
            }

            ((PxRigidBody*)actor)->AddForceMut((PxVec3*)&force, Helper.Convert(mode), autoAwake);
        }

        public void AddTorque(Vector3 torque, ForceMode mode, bool autoAwake = true)
        {
            if (type == ActorType.Static)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot add torque for static actors.");
            }

            ((PxRigidBody*)actor)->AddTorqueMut((PxVec3*)&torque, Helper.Convert(mode), autoAwake);
        }

        public void ClearForce(ForceMode mode)
        {
            if (type == ActorType.Static)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot clear force for static actors.");
            }

            ((PxRigidBody*)actor)->ClearForceMut(Helper.Convert(mode));
        }

        public void ClearTorque(ForceMode mode)
        {
            if (type == ActorType.Static)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot clear torque for static actors.");
            }

            ((PxRigidBody*)actor)->ClearTorqueMut(Helper.Convert(mode));
        }

        public void SetForceAndTorque(Vector3 force, Vector3 torque, ForceMode mode)
        {
            if (type == ActorType.Static)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot set force and torque for static actors.");
            }

            ((PxRigidBody*)actor)->SetForceAndTorqueMut((PxVec3*)&force, (PxVec3*)&torque, Helper.Convert(mode));
        }

        public void SetKinematicTarget(Vector3 targetPosition, Quaternion targetRotation)
        {
            if (type != ActorType.Kinematic)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot set kinematic target for non kinematic actors.");
            }

            PxTransform destination = new() { p = targetPosition, q = targetRotation };
            ((PxRigidDynamic*)actor)->SetKinematicTargetMut(&destination);
        }

        public void GetKinematicTarget(out Vector3 targetPosition, out Quaternion targetRotation)
        {
            if (type != ActorType.Kinematic)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot set kinematic target for non kinematic actors.");
            }

            PxTransform destination;
            ((PxRigidDynamic*)actor)->GetKinematicTarget(&destination);
            targetPosition = destination.p;
            targetRotation = destination.q;
        }

        public void SetSolverIterationCounts(uint minPositionIters, uint minVelocityIters = 1)
        {
            if (type == ActorType.Static)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot execute for static actors.");
            }

            ((PxRigidDynamic*)actor)->SetSolverIterationCountsMut(minPositionIters, minVelocityIters);
        }

        public void GetSolverIterationCounts(out uint minPositionIters, out uint minVelocityIters)
        {
            if (type == ActorType.Static)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot execute for static actors.");
            }

            uint vminPositionIters;
            uint vminVelocityIters;
            ((PxRigidDynamic*)actor)->GetSolverIterationCounts(&vminPositionIters, &vminVelocityIters);
            minPositionIters = vminPositionIters;
            minVelocityIters = vminVelocityIters;
        }

        public void SetCMassLocalPose(Vector3 position, Quaternion rotation)
        {
            if (type == ActorType.Static)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot execute for static actors.");
            }

            PxTransform pose = new() { p = position, q = rotation };
            ((PxRigidBody*)actor)->SetCMassLocalPoseMut(&pose);
        }

        public void GetCMassLocalPose(out Vector3 position, out Quaternion rotation)
        {
            if (type == ActorType.Static)
            {
                throw new InvalidOperationException($"{GameObject.FullName}: Cannot execute for static actors.");
            }

            PxTransform pose = ((PxRigidBody*)actor)->GetCMassLocalPose();
            position = pose.p;
            rotation = pose.q;
        }
    }
}