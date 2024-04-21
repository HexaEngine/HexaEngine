namespace HexaEngine.Components.Physics
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Physics;
    using HexaEngine.Scenes;
    using MagicPhysX;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    [EditorCategory("Physics")]
    [EditorComponent<CharacterController>("Character Controller")]
    public sealed unsafe class CharacterController : ICharacterControllerComponent
    {
        protected PxMaterial* material;
        private CharacterControllerShape shape;

        private float dynamicFriction = 0.5f;
        private float staticFriction = 0.5f;
        private float restitution = 0.6f;

        internal PxController* controller;
        private float capsuleHeight = 1;
        private float capsuleRadius = 1f;
        private float boxDepth = 1;
        private float boxHeight = 1;
        private float boxWidth = 1;
        private float density = 10.0f;
        private float maxJumpHeight = 0;
        private float contactOffset = 0.1f;
        private float stepOffset = 0.5f;
        private float slopeLimit = 0.707f;
        private float volumeGrowth = 1.5f;
        private float invisibleWallHeight = 0f;
        private float scaleCoeff = 0.8f;
        private CapsuleClimbingMode capsuleClimbingMode;
        private ControllerNonWalkableMode nonWalkableMode;

        private PxRigidDynamic* actor;
        private RigidBody rigidBody;
        private bool isGrounded;

        /// <summary>
        /// The GUID of the <see cref="CharacterController"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        public Guid Guid { get; set; } = Guid.NewGuid();

        [EditorProperty<CharacterControllerShape>("Shape")]
        public CharacterControllerShape Shape { get => shape; set => shape = value; }

        [EditorPropertyCondition<CharacterController>(nameof(IsCapsule))]
        [EditorCategory("Capsule")]
        [EditorProperty("Height")]
        public float CapsuleHeight
        {
            get => capsuleHeight;
            set
            {
                capsuleHeight = value;
                if (controller != null && shape == CharacterControllerShape.Capsule)
                {
                    ((PxCapsuleController*)controller)->SetHeightMut(value);
                }
            }
        }

        [EditorPropertyCondition<CharacterController>(nameof(IsCapsule))]
        [EditorCategory("Capsule")]
        [EditorProperty("Radius")]
        public float CapsuleRadius
        {
            get => capsuleRadius;
            set
            {
                capsuleRadius = value;
                if (controller != null && shape == CharacterControllerShape.Capsule)
                {
                    ((PxCapsuleController*)controller)->SetRadiusMut(value);
                }
            }
        }

        [EditorPropertyCondition<CharacterController>(nameof(IsBox))]
        [EditorCategory("Box")]
        [EditorProperty("Width")]
        public float BoxWidth
        {
            get => boxWidth;
            set
            {
                boxWidth = value;
                if (controller != null && shape == CharacterControllerShape.Box)
                {
                    ((PxBoxController*)controller)->SetHalfSideExtentMut(value);
                }
            }
        }

        [EditorPropertyCondition<CharacterController>(nameof(IsBox))]
        [EditorCategory("Box")]
        [EditorProperty("Height")]
        public float BoxHeight
        {
            get => boxHeight;
            set
            {
                boxHeight = value;
                if (controller != null && shape == CharacterControllerShape.Box)
                {
                    ((PxBoxController*)controller)->SetHalfHeightMut(value);
                }
            }
        }

        [EditorPropertyCondition<CharacterController>(nameof(IsBox))]
        [EditorCategory("Box")]
        [EditorProperty("Depth")]
        public float BoxDepth
        {
            get => boxDepth;
            set
            {
                boxDepth = value;
                if (controller != null && shape == CharacterControllerShape.Box)
                {
                    ((PxBoxController*)controller)->SetHalfForwardExtentMut(value);
                }
            }
        }

        [EditorProperty("Density")]
        public float Density
        {
            get => density;
            set
            {
                density = value;
            }
        }

        [EditorProperty("Static Friction")]
        public float StaticFriction
        {
            get => staticFriction;
            set
            {
                staticFriction = value;
            }
        }

        [EditorProperty("Dynamic Friction")]
        public float DynamicFriction
        {
            get => dynamicFriction;
            set
            {
                dynamicFriction = value;
            }
        }

        [EditorProperty("Restitution")]
        public float Restitution
        {
            get => restitution;
            set
            {
                restitution = value;
            }
        }

        [EditorProperty("Step Offset")]
        public float StepOffset
        {
            get => stepOffset;
            set
            {
                stepOffset = value;
                if (controller != null)
                {
                    controller->SetStepOffsetMut(value);
                }
            }
        }

        [EditorProperty<ControllerNonWalkableMode>("Non Walkable Mode")]
        public ControllerNonWalkableMode NonWalkableMode
        {
            get => nonWalkableMode;
            set
            {
                nonWalkableMode = value;
                if (controller != null)
                {
                    controller->SetNonWalkableModeMut(Helper.Convert(value));
                }
            }
        }

        [EditorProperty("Contact Offset")]
        public float ContactOffset
        {
            get => contactOffset;
            set
            {
                contactOffset = value;
                if (controller != null)
                {
                    controller->SetContactOffsetMut(value);
                }
            }
        }

        [EditorProperty("Slope Limit")]
        public float SlopeLimit
        {
            get => slopeLimit;
            set
            {
                slopeLimit = value;
                if (controller != null)
                {
                    controller->SetSlopeLimitMut(value);
                }
            }
        }

        [EditorProperty("Max Jump Height")]
        public float MaxJumpHeight
        {
            get => maxJumpHeight;
            set
            {
                maxJumpHeight = value;
            }
        }

        [EditorProperty("Volume Growth")]
        public float VolumeGrowth { get => volumeGrowth; set => volumeGrowth = value; }

        [EditorProperty("Invisible Wall Height")]
        public float InvisibleWallHeight { get => invisibleWallHeight; set => invisibleWallHeight = value; }

        [EditorProperty("Scale Coeff")]
        public float ScaleCoeff { get => scaleCoeff; set => scaleCoeff = value; }

        [EditorPropertyCondition<CharacterController>(nameof(IsCapsule))]
        [EditorCategory("Capsule")]
        [EditorProperty<CapsuleClimbingMode>("Capsule Climbing Mode")]
        public CapsuleClimbingMode CapsuleClimbingMode { get => capsuleClimbingMode; set => capsuleClimbingMode = value; }

        [JsonIgnore]
        public GameObject GameObject { get; set; } = null!;

        [JsonIgnore]
        public Vector3D Position
        {
            get
            {
                if (controller == null)
                {
                    return default;
                }

                return *(Vector3D*)controller->GetPosition();
            }
            set
            {
                if (controller == null)
                {
                    return;
                }

                controller->SetPositionMut((PxExtendedVec3*)&value);
            }
        }

        [JsonIgnore]
        public Vector3D FootPosition
        {
            get
            {
                if (controller == null)
                {
                    return default;
                }

                return Unsafe.BitCast<PxExtendedVec3, Vector3D>(controller->GetFootPosition());
            }
            set
            {
                if (controller == null)
                {
                    return;
                }

                controller->SetFootPositionMut((PxExtendedVec3*)&value);
            }
        }

        [JsonIgnore]
        public Vector3 UpDirection
        {
            get
            {
                if (controller == null)
                {
                    return default;
                }
                return controller->GetUpDirection();
            }
            set
            {
                if (controller == null)
                {
                    return;
                }

                controller->SetUpDirectionMut((PxVec3*)&value);
            }
        }

        [JsonIgnore]
        public RigidBody Actor => rigidBody;

        [JsonIgnore]
        public Vector3 AngularVelocity
        {
            get
            {
                if (actor == null)
                {
                    return Vector3.Zero;
                }

                return actor->GetAngularVelocity();
            }
        }

        [JsonIgnore]
        public Vector3 LinearVelocity
        {
            get
            {
                if (actor == null)
                {
                    return Vector3.Zero;
                }

                return actor->GetLinearVelocity();
            }
        }

        [JsonIgnore]
        public bool IsGrounded
        {
            get => isGrounded;
        }

        public static bool IsCapsule(CharacterController controller)
        {
            return controller.shape == CharacterControllerShape.Capsule;
        }

        public static bool IsBox(CharacterController controller)
        {
            return controller.shape == CharacterControllerShape.Box;
        }

        void IComponent.Awake()
        {
        }

        void IComponent.Destroy()
        {
        }

        void ICharacterControllerComponent.CreateController(PxPhysics* physics, PxScene* scene, PxControllerManager* manager)
        {
            material = physics->CreateMaterialMut(staticFriction, dynamicFriction, restitution);

            var transform = GameObject.Transform;

            switch (shape)
            {
                case CharacterControllerShape.Capsule:
                    PxCapsuleControllerDesc* capsuleControllerDesc = NativeMethods.PxCapsuleControllerDesc_new_alloc();
                    capsuleControllerDesc->SetToDefaultMut();
                    capsuleControllerDesc->climbingMode = Helper.Convert(capsuleClimbingMode);
                    capsuleControllerDesc->height = capsuleHeight * 2;
                    capsuleControllerDesc->radius = capsuleRadius;
                    capsuleControllerDesc->position = new() { x = transform.GlobalPosition.X, y = transform.GlobalPosition.Y, z = transform.GlobalPosition.Z };
                    capsuleControllerDesc->upDirection = GameObject.Transform.Up;
                    capsuleControllerDesc->density = density;
                    capsuleControllerDesc->maxJumpHeight = maxJumpHeight;
                    capsuleControllerDesc->nonWalkableMode = Helper.Convert(nonWalkableMode);
                    capsuleControllerDesc->contactOffset = contactOffset;
                    capsuleControllerDesc->stepOffset = stepOffset;
                    capsuleControllerDesc->slopeLimit = slopeLimit;
                    capsuleControllerDesc->volumeGrowth = volumeGrowth;
                    capsuleControllerDesc->invisibleWallHeight = invisibleWallHeight;
                    capsuleControllerDesc->scaleCoeff = scaleCoeff;
                    capsuleControllerDesc->material = material;

                    if (!capsuleControllerDesc->IsValid())
                    {
                        PhysicsSystem.Logger.Error("Couldn't create character controller, invalid parameters.");
                        capsuleControllerDesc->Delete();
                        return;
                    }

                    controller = manager->CreateControllerMut((PxControllerDesc*)capsuleControllerDesc);
                    capsuleControllerDesc->Delete();
                    break;

                case CharacterControllerShape.Box:
                    PxBoxControllerDesc* boxControllerDesc = NativeMethods.PxBoxControllerDesc_new_alloc();

                    boxControllerDesc->halfSideExtent = boxWidth;
                    boxControllerDesc->halfHeight = boxHeight;
                    boxControllerDesc->halfForwardExtent = boxDepth;
                    boxControllerDesc->position = new() { x = transform.GlobalPosition.X, y = transform.GlobalPosition.Y, z = transform.GlobalPosition.Z };
                    boxControllerDesc->upDirection = GameObject.Transform.Up;
                    boxControllerDesc->density = density;
                    boxControllerDesc->maxJumpHeight = maxJumpHeight;
                    boxControllerDesc->nonWalkableMode = Helper.Convert(nonWalkableMode);
                    boxControllerDesc->contactOffset = contactOffset;
                    boxControllerDesc->stepOffset = stepOffset;
                    boxControllerDesc->slopeLimit = slopeLimit;
                    boxControllerDesc->volumeGrowth = volumeGrowth;
                    boxControllerDesc->invisibleWallHeight = invisibleWallHeight;
                    boxControllerDesc->scaleCoeff = scaleCoeff;
                    boxControllerDesc->material = material;

                    if (!boxControllerDesc->IsValid())
                    {
                        PhysicsSystem.Logger.Error("Couldn't create character controller, invalid parameters.");
                        boxControllerDesc->Delete();
                        return;
                    }

                    controller = manager->CreateControllerMut((PxControllerDesc*)boxControllerDesc);
                    boxControllerDesc->Delete();
                    break;

                default:
                    PhysicsSystem.Logger.Error($"CharacterControllerShape ({Shape}) is not supported.");
                    return;
            }

            actor = controller->GetActor();
            rigidBody = new(actor, ActorType.Kinematic, this);
            Physics.Actor.mapper.AddMapping(actor, rigidBody);
        }

        void ICharacterControllerComponent.DestroyController()
        {
            if (material != null)
            {
                ((PxBase*)material)->ReleaseMut();
                material = null;
            }

            if (controller != null)
            {
                Physics.Actor.mapper.RemoveMapping(actor);
                rigidBody.CharacterController = null; // dereference here.
                rigidBody = null;
                actor = null;
                controller->ReleaseMut();
                controller = null;
            }
        }

        void ICharacterControllerComponent.Update()
        {
            PxExtendedVec3* pos = controller->GetPosition();
            GameObject.Transform.GlobalPosition = new((float)pos->x, (float)pos->y, (float)pos->z);
        }

        public ControllerCollisionFlags Move(Vector3 displacement, float minDistance, float elapsedTime)
        {
            ControllerFilters filters = default;
            filters.FilterFlags = QueryFlags.Static | QueryFlags.Dynamic;
            return Move(displacement, minDistance, elapsedTime, filters);
        }

        public ControllerCollisionFlags Move(Vector3 displacement, float minDistance, float elapsedTime, ControllerFilters controllerFilters)
        {
            PxControllerFilters filters = controllerFilters.filters;
            PxControllerCollisionFlags flags = controller->MoveMut((PxVec3*)&displacement, minDistance, elapsedTime, &filters, null);
            isGrounded = (flags & PxControllerCollisionFlags.CollisionDown) != 0;
            return Helper.Convert(flags);
        }

        public void InvalidateCache()
        {
            controller->InvalidateCacheMut();
        }

        public void Resize(float height)
        {
            controller->ResizeMut(height);
        }
    }
}