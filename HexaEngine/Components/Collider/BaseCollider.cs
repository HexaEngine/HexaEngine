namespace HexaEngine.Components.Collider
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using BepuUtilities.Memory;
    using HexaEngine.Core;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Physics;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Systems;

    public abstract class BaseCollider : IColliderComponent
    {
        protected bool hasBody = false;
        protected bool hasShape = false;
        protected bool inCompound = false;
        protected bool update = true;

        protected TypedIndex index;
        protected StaticHandle staticHandle;
        protected BodyHandle bodyHandle;
        protected BodyReference bodyReference;
        protected RigidPose pose;
        protected BodyInertia inertia;
        protected PhysicsMaterial material;

        protected Scene? scene;
        protected PhysicsSystem? system;
        protected Simulation? simulation;
        protected BufferPool? bufferPool;
        protected ColliderType type;

        protected ICompoundCollider? parentCollider;
        protected CompoundChild? compoundChild;

        private float mass = 1;
        private float sleepThreshold = 0.01f;
        private bool lockRotation;
        private float springFrequency = 1;
        private float springDampingRatio = 30;
        private float frictionCoefficient = 1;
        private float maximumRecoveryVelocity = 2;

        [EditorProperty<ColliderType>("Type")]
        public ColliderType Type
        { get => type; set { type = value; update = true; } }

        [EditorProperty("Mass")]
        public float Mass
        { get => mass; set { mass = value; update = true; } }

        [EditorProperty("Sleep threshold")]
        public float SleepThreshold
        { get => sleepThreshold; set { sleepThreshold = value; update = true; } }

        [EditorProperty("Lock Rotation")]
        public bool LockRotation
        { get => lockRotation; set { lockRotation = value; update = true; } }

        [EditorProperty("Spring Frequency")]
        public float SpringFrequency
        { get => springFrequency; set { springFrequency = value; } }

        [EditorProperty("Spring Damping Ratio")]
        public float SpringDampingRatio
        { get => springDampingRatio; set { springDampingRatio = value; } }

        [EditorProperty("Friction Coefficient")]
        public float FrictionCoefficient
        { get => frictionCoefficient; set { frictionCoefficient = value; } }

        [EditorProperty("Max Recovery Velocity")]
        public float MaximumRecoveryVelocity
        { get => maximumRecoveryVelocity; set { maximumRecoveryVelocity = value; } }

        [JsonIgnore]
        public TypedIndex ShapeIndex => index;

        [JsonIgnore]
        public StaticHandle StaticHandle => staticHandle;

        [JsonIgnore]
        public BodyHandle BodyHandle => bodyHandle;

        [JsonIgnore]
        public PhysicsMaterial Material => material;

        [JsonIgnore]
        public ICompoundCollider? ParentCollider => parentCollider;

        [JsonIgnore]
        public CompoundChild? CompoundChild => compoundChild;

        [JsonIgnore]
        public bool HasBody => hasBody;

        [JsonIgnore]
        public bool HasShape => hasShape;

        [JsonIgnore]
        public bool InCompound => inCompound;

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        public virtual void Awake()
        {
            scene = GameObject.GetScene();
            system = scene.GetRequiredSystem<PhysicsSystem>();
            simulation = system.Simulation;
            bufferPool = system.BufferPool;
            CreateBody();
        }

        public virtual void Destroy()
        {
            DestroyBody();
        }

        public virtual void BeginUpdate()
        {
            if (update)
            {
                CreateBody();
            }

            if (type != ColliderType.Static && GameObject != null)
            {
                (bodyReference.Pose.Position, bodyReference.Pose.Orientation) = GameObject.Transform.PositionRotation;
            }
        }

        public virtual void EndUpdate()
        {
            if (update)
            {
                CreateBody();
            }

            if (type != ColliderType.Static && GameObject != null)
            {
                GameObject.Transform.PositionRotation = (bodyReference.Pose.Position, bodyReference.Pose.Orientation);
            }
        }

        public abstract void CreateShape();

        public virtual void DestroyShape()
        {
            if (Application.InDesignMode || GameObject == null || simulation == null || !hasShape)
            {
                return;
            }

            lock (simulation)
            {
                simulation.Shapes.Remove(index);
            }

            hasShape = false;
        }

        public virtual void CreateBody()
        {
            if (Application.InDesignMode || GameObject == null || simulation == null || inCompound || hasBody)
            {
                return;
            }

            DestroyBody();
            CreateShape();

            if (lockRotation)
            {
                inertia = default;
            }

            lock (simulation)
            {
                if (Type == ColliderType.Static)
                {
                    staticHandle = simulation.Statics.Add(new(pose, index));
                    system.Materials.Allocate(staticHandle) = new(new(springFrequency, springDampingRatio), frictionCoefficient, maximumRecoveryVelocity);
                }
                if (Type == ColliderType.Dynamic)
                {
                    bodyHandle = simulation.Bodies.Add(BodyDescription.CreateDynamic(pose, default, inertia, new CollidableDescription(index), new(SleepThreshold)));
                    bodyReference = simulation.Bodies.GetBodyReference(bodyHandle);
                    system.Materials.Allocate(bodyHandle) = new(new(springFrequency, springDampingRatio), frictionCoefficient, maximumRecoveryVelocity);
                }
                if (Type == ColliderType.Kinematic)
                {
                    bodyHandle = simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, default, new CollidableDescription(index), new(SleepThreshold)));
                    bodyReference = simulation.Bodies.GetBodyReference(bodyHandle);
                    system.Materials.Allocate(bodyHandle) = new(new(springFrequency, springDampingRatio), frictionCoefficient, maximumRecoveryVelocity);
                }
            }

            hasBody = true;
            update = false;
        }

        public virtual void DestroyBody()
        {
            if (Application.InDesignMode || GameObject == null || simulation == null || !hasBody)
            {
                return;
            }

            hasBody = false;
            lock (simulation)
            {
                if (Type == ColliderType.Static)
                {
                    simulation.Statics.Remove(staticHandle);
                }
                else
                {
                    simulation.Bodies.Remove(bodyHandle);
                }
            }

            DestroyShape();
        }

        public virtual void BuildCompound(ref CompoundBuilder builder)
        {
            if (Application.InDesignMode || GameObject == null || inCompound)
            {
                return;
            }

            if (hasBody)
            {
                DestroyBody();
            }
            if (hasShape)
            {
                DestroyShape();
            }

            CreateShape();
            builder.Add(index, pose, inertia.InverseInertiaTensor, mass);
            inCompound = true;
        }

        public virtual void SetCompoundData(ICompoundCollider parentCollider, CompoundChild compoundChild)
        {
            this.parentCollider = parentCollider;
            this.compoundChild = compoundChild;
        }

        public virtual void DestroyCompound()
        {
            if (parentCollider == null || compoundChild == null || !inCompound)
            {
                return;
            }

            DestroyShape();
            parentCollider = null;
            compoundChild = null;
            inCompound = false;
        }
    }
}