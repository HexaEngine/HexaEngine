namespace HexaEngine.Scenes.Components
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Scenes.Components.Collider;
    using System.Numerics;

    [EditorComponent<TriangleCollider>("Triangle Collider")]
    public class TriangleCollider : IComponent
    {
        private bool init = false;
        private bool update = true;
        private GameObject? node;
        private Scene? scene;
        private TypedIndex index;
        private StaticHandle staticHandle;
        private BodyHandle bodyHandle;
        private BodyReference bodyReference;
        private ColliderType type;
        private float mass = 1;
        private Vector3 pos1;
        private Vector3 pos2;
        private Vector3 pos3;
        private float sleepThreshold = 0.01f;

        [EditorProperty<ColliderType>("Type")]
        public ColliderType Type
        { get => type; set { type = value; update = true; } }

        [EditorProperty("Pos 1")]
        public Vector3 Pos1
        { get => pos1; set { pos1 = value; update = true; } }

        [EditorProperty("Pos 2")]
        public Vector3 Pos2
        { get => pos2; set { pos2 = value; update = true; } }

        [EditorProperty("Pos 3")]
        public Vector3 Pos3
        { get => pos3; set { pos3 = value; update = true; } }

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
            Triangle triangle = new(pos1, pos2, pos3);
            RigidPose pose = new(node.Transform.GlobalPosition, node.Transform.GlobalOrientation);
            index = scene.Simulation.Shapes.Add(triangle);
            if (Type == ColliderType.Static)
            {
                staticHandle = scene.Simulation.Statics.Add(new(pose, index));
            }
            if (Type == ColliderType.Dynamic)
            {
                var inertia = triangle.ComputeInertia(mass);
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

        public void Awake(IGraphicsDevice device, GameObject node)
        {
            this.node = node;
            scene = node.GetScene();
            Init();
        }

        public void Destory()
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