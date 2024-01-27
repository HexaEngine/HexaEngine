namespace HexaEngine.Components
{
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Physics;
    using HexaEngine.Scenes;
    using MagicPhysX;
    using System.Numerics;

    [EditorCategory("Physics")]
    [EditorComponent<CharacterController>("Character Controller")]
    public unsafe class CharacterController : IComponent
    {
        private CharacterControllerShape shape;

        private Scene scene;
        private PhysicsSystem system;
        private ControllerManager manager;
        private PxController* controller;
        private float height = 1;
        private float radius = 0.5f;
        private float density = 1;
        private float maxJumpHeight = 1;
        private float contactOffset;
        private float stepOffset;
        private float slopeLimit;
        private float volumeGrowth;
        private float invisibleWallHeight;
        private float scaleCoeff;
        private CapsuleClimbingMode capsuleClimbingMode;
        private ControllerNonWalkableMode nonWalkableMode;

        [EditorProperty<CharacterControllerShape>("Shape")]
        public CharacterControllerShape Shape { get => shape; set => shape = value; }

        public float Height { get => height; set => height = value; }

        public float Radius { get => radius; set => radius = value; }

        public float Density { get => density; set => density = value; }

        public float MaxJumpHeight { get => maxJumpHeight; set => maxJumpHeight = value; }

        public float ContactOffset { get => contactOffset; set => contactOffset = value; }

        public float StepOffset { get => stepOffset; set => stepOffset = value; }

        public float SlopeLimit { get => slopeLimit; set => slopeLimit = value; }

        public float VolumeGrowth { get => volumeGrowth; set => volumeGrowth = value; }

        public float InvisibleWallHeight { get => invisibleWallHeight; set => invisibleWallHeight = value; }

        public float ScaleCoeff { get => scaleCoeff; set => scaleCoeff = value; }

        public CapsuleClimbingMode CapsuleClimbingMode { get => capsuleClimbingMode; set => capsuleClimbingMode = value; }

        public ControllerNonWalkableMode NonWalkableMode { get => nonWalkableMode; set => nonWalkableMode = value; }

        public GameObject GameObject { get; set; }

        public void Awake()
        {
            scene = GameObject.GetScene();
            system = scene.GetRequiredSystem<PhysicsSystem>();
            manager = system.ControllerManager;

            var transform = GameObject.Transform;

            switch (shape)
            {
                case CharacterControllerShape.Capsule:
                    PxCapsuleControllerDesc capsuleControllerDesc = new()
                    {
                        climbingMode = Helper.Convert(capsuleClimbingMode),
                        height = height,
                        radius = radius,
                        position = new() { x = transform.GlobalPosition.X, y = transform.GlobalPosition.Y, z = transform.GlobalPosition.Z },
                        upDirection = GameObject.Transform.Up,
                        density = density,
                        maxJumpHeight = maxJumpHeight,
                        nonWalkableMode = Helper.Convert(nonWalkableMode),
                        contactOffset = contactOffset,
                        stepOffset = stepOffset,
                        slopeLimit = slopeLimit,
                        volumeGrowth = volumeGrowth,
                        invisibleWallHeight = invisibleWallHeight,
                        scaleCoeff = scaleCoeff,
                    };

                    controller = manager.CreateController(capsuleControllerDesc);
                    break;

                case CharacterControllerShape.Box:
                    PxBoxControllerDesc boxControllerDesc = new()
                    {
                        position = new() { x = transform.GlobalPosition.X, y = transform.GlobalPosition.Y, z = transform.GlobalPosition.Z },
                        upDirection = GameObject.Transform.Up,
                        density = density,
                        maxJumpHeight = maxJumpHeight,
                        nonWalkableMode = Helper.Convert(nonWalkableMode),
                        contactOffset = contactOffset,
                        stepOffset = stepOffset,
                        slopeLimit = slopeLimit,
                        volumeGrowth = volumeGrowth,
                        invisibleWallHeight = invisibleWallHeight,
                        scaleCoeff = scaleCoeff,
                    };

                    controller = manager.CreateController(boxControllerDesc);
                    break;

                default:
                    throw new NotSupportedException($"CharacterControllerShape ({Shape}) is not supported.");
            }
        }

        public void Destroy()
        {
            controller->ReleaseMut();
            controller = null;
        }

        public void Move(Vector3 displacement, float minDistance, float elapsedTime)
        {
            controller->MoveMut((PxVec3*)&displacement, minDistance, elapsedTime, null, null);
        }
    }
}