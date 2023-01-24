namespace HexaEngine.Scenes.Components.Collider
{
    using BepuPhysics.Collidables;
    using BepuPhysics;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using HexaEngine.Core;
    using System.IO;
    using HexaEngine.Core.IO;
    using System.Numerics;
    using BepuPhysics.CollisionDetection;
    using BepuPhysics.CollisionDetection.CollisionTasks;

    [EditorComponent(typeof(ConvexCollider), "Convex Collider")]
    public class ConvexCollider : IComponent
    {
        public static ConvexCollider Con()
        {
            return new ConvexCollider();
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
        private float mass = 1;
        private string mesh = string.Empty;
        private float sleepThreshold = 0.01f;
        private ConvexHull? convexHull;
        private HullData? hullData;
        private Vector3 hullCenter;
        private Vector3[] points;

        [EditorProperty<ColliderType>("Type")]
        public ColliderType Type
        { get => type; set { type = value; update = true; } }

        [EditorProperty("Mesh")]
        public string Mesh
        { get => mesh; set { mesh = value; update = true; } }

        [EditorProperty("Mass")]
        public float Mass
        { get => mass; set { mass = value; update = true; } }

        [EditorProperty("Sleep threshold")]
        public float SleepThreshold
        { get => sleepThreshold; set { sleepThreshold = value; update = true; } }

        [JsonIgnore]
        public ConvexHull? ConvexHull => convexHull;

        [JsonIgnore]
        public HullData? HullData => hullData;

        [JsonIgnore]
        public Vector3[] Points => points;

        [JsonIgnore]
        public Vector3 HullCenter => hullCenter;

        [JsonIgnore]
        public TypedIndex ShapeIndex => index;

        [JsonIgnore]
        public StaticHandle StaticHandle => staticHandle;

        [JsonIgnore]
        public BodyHandle BodyHandle => bodyHandle;

        [EditorButton("Bake shape")]
        public void BakeShape()
        {
            if (scene == null) return;
            convexHull?.Dispose(scene.BufferPool);
            var data = scene.MeshManager.Load(mesh);
            points = data.GetPoints();

            ConvexHullHelper.CreateShape(points, scene.BufferPool, out var dat, out hullCenter, out var ull);
            hullData = dat;
            convexHull = ull;
        }

        private void Init()
        {
            if (Application.InDesignMode || node == null || scene == null) return;
            if (!FileSystem.Exists(Path.Combine(Paths.CurrentMeshesPath, mesh + ".mesh")))
                return;
            Uninit();
            update = false;
            init = true;

            if (convexHull != null)
            {
                var ull = convexHull.Value;
                RigidPose pose = new(node.Transform.GlobalPosition, node.Transform.GlobalOrientation);
                index = scene.Simulation.Shapes.Add(ull);
                if (Type == ColliderType.Static)
                {
                    staticHandle = scene.Simulation.Statics.Add(new(pose, index));
                }
                if (Type == ColliderType.Dynamic)
                {
                    var inertia = ull.ComputeInertia(mass);
                    bodyHandle = scene.Simulation.Bodies.Add(BodyDescription.CreateDynamic(pose, new BodyVelocity(), inertia, new CollidableDescription(index), new(sleepThreshold)));
                    bodyReference = scene.Simulation.Bodies.GetBodyReference(bodyHandle);
                }
                if (Type == ColliderType.Kinematic)
                {
                    bodyHandle = scene.Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, new BodyVelocity(), new CollidableDescription(index), new(sleepThreshold)));
                    bodyReference = scene.Simulation.Bodies.GetBodyReference(bodyHandle);
                }
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