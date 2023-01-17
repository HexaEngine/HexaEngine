namespace HexaEngine.Scenes.Components
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Scenes.Components.Collider;

    public abstract class ColliderBase : IComponent
    {
        protected bool init = false;
        protected bool update = true;

        protected GameObject? node;
        protected Scene? scene;
        protected ColliderType type;

        [EditorProperty<ColliderType>("Type")]
        public ColliderType Type
        { get => type; set { type = value; update = true; } }

        public abstract void Awake(IGraphicsDevice device, GameObject gameObject);

        public abstract void Destory();
    }

    [EditorComponent<CapsuleCollider>("Capsule Collider")]
    public class CapsuleCollider : ColliderBase
    {
        private TypedIndex index;
        private StaticHandle staticHandle;
        private BodyHandle bodyHandle;
        private BodyReference bodyReference;

        private float mass = 1;
        private float radius = 1;
        private float length = 2;
        private float sleepThreshold = 0.01f;

        [EditorProperty("Radius")]
        public float Radius
        { get => radius; set { radius = value; update = true; } }

        [EditorProperty("Length")]
        public float Length
        { get => length; set { length = value; update = true; } }

        [EditorProperty("Mass")]
        public float Mass
        { get => mass; set { mass = value; update = true; } }

        [EditorProperty("Sleep threshold")]
        public float SleepThreshold
        { get => sleepThreshold; set { sleepThreshold = value; update = true; } }

        [JsonIgnore]
        public TypedIndex ShapeIndex => index;

        [JsonIgnore]
        public StaticHandle StaticHandle => staticHandle;

        [JsonIgnore]
        public BodyHandle BodyHandle => bodyHandle;

        private void Init()
        {
            if (Application.InDesignMode || node == null || scene == null) return;
            Uninit();
            update = false;
            init = true;
            Capsule capsule = new(radius, length);
            RigidPose pose = new(node.Transform.GlobalPosition, node.Transform.GlobalOrientation);
            index = scene.Simulation.Shapes.Add(capsule);
            if (Type == ColliderType.Static)
            {
                staticHandle = scene.Simulation.Statics.Add(new(pose, index));
            }
            if (Type == ColliderType.Dynamic)
            {
                var inertia = capsule.ComputeInertia(mass);
                bodyHandle = scene.Simulation.Bodies.Add(BodyDescription.CreateDynamic(pose, new BodyVelocity(), inertia, new CollidableDescription(index), new(sleepThreshold)));
                bodyReference = scene.Simulation.Bodies.GetBodyReference(bodyHandle);
            }
            if (Type == ColliderType.Kinematic)
            {
                bodyHandle = scene.Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, new BodyVelocity(), new CollidableDescription(index), new(sleepThreshold)));
                bodyReference = scene.Simulation.Bodies.GetBodyReference(bodyHandle);
            }
        }

        private void Uninit()
        {
            if (Application.InDesignMode || node == null || scene == null) return;
            if (!init) return;
            init = false;
            if (Type == ColliderType.Static)
                scene.Simulation.Statics.Remove(staticHandle);
            else
                scene.Simulation.Bodies.Remove(bodyHandle);
            scene.Simulation.Shapes.Remove(index);
        }

        public override void Awake(IGraphicsDevice device, GameObject node)
        {
            this.node = node;
            scene = node.GetScene();
            Init();
        }

        public override void Destory()
        {
            Uninit();
        }

        public void Update()
        {
            if (Application.InDesignMode) return;
            if (update)
                Init();
            if (type != ColliderType.Static && node != null)
            {
                node.Transform.PositionRotation = (bodyReference.Pose.Position, bodyReference.Pose.Orientation);
            }
        }
    }
}