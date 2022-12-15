namespace HexaEngine.Objects.Components
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;

    [EditorComponent(typeof(BoxCollider), "Box Collider")]
    public class BoxCollider : IComponent
    {
        public static BoxCollider Con()
        {
            return new BoxCollider();
        }

        private bool init = false;
        private bool update = true;
        private GameObject? node;
        private Scene? scene;
        private TypedIndex index;
        private StaticHandle staticHandle;
        private BodyHandle bodyHandle;
        private BodyReference bodyReference;
        private ColliderType type;
        private float height = 1;
        private float depth = 1;
        private float mass = 1;
        private float width = 1;
        private float sleepThreshold = 0.01f;

        public BoxCollider()
        {
            Editor = new PropertyEditor<BoxCollider>(this);
        }

        public IPropertyEditor? Editor { get; }

        [EditorProperty<ColliderType>("Type")]
        public ColliderType Type
        { get => type; set { type = value; update = true; } }

        [EditorProperty("Width")]
        public float Width
        { get => width; set { width = value; update = true; } }

        [EditorProperty("Height")]
        public float Height
        { get => height; set { height = value; update = true; } }

        [EditorProperty("Depth")]
        public float Depth
        { get => depth; set { depth = value; update = true; } }

        [EditorProperty("Mass")]
        public float Mass
        { get => mass; set { mass = value; update = true; } }

        [EditorProperty("Sleep threshold")]
        public float SleepThreshold
        { get => sleepThreshold; set { sleepThreshold = value; update = true; } }

        public TypedIndex ShapeIndex => index;

        public StaticHandle StaticHandle => staticHandle;

        public BodyHandle BodyHandle => bodyHandle;

        private void Init()
        {
            if (Designer.InDesignMode || node == null || scene == null) return;
            Uninit();
            update = false;
            init = true;
            Box box = new(width * 2, height * 2, depth * 2);
            RigidPose pose = new(node.Transform.GlobalPosition, node.Transform.GlobalOrientation);
            index = scene.Simulation.Shapes.Add(box);
            if (Type == ColliderType.Static)
            {
                staticHandle = scene.Simulation.Statics.Add(new(pose, index));
            }
            if (Type == ColliderType.Dynamic)
            {
                var inertia = box.ComputeInertia(mass);
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
            if (Designer.InDesignMode || node == null || scene == null) return;
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
            if (Designer.InDesignMode) return;
            if (update)
                Init();
            if (type != ColliderType.Static && node != null)
            {
                node.Transform.PositionRotation = (bodyReference.Pose.Position, bodyReference.Pose.Orientation);
            }
        }
    }
}