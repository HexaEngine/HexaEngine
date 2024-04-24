namespace HexaEngine.Components.Physics.Collider
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Physics;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Serialization;
    using MagicPhysX;
    using System.Numerics;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Physics.Collider.ColliderShape")]
    public abstract unsafe class ColliderShape : IColliderComponent
    {
        protected PxMaterial* material;

        private float density = 1;
        private float dynamicFriction = 0.5f;
        private float staticFriction = 0.5f;
        private float restitution = 0.6f;
        private PxTransform localPose;

        private readonly List<Pointer<PxShape>> shapes = new();

        /// <summary>
        /// The GUID of the <see cref="ColliderShape"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        public Guid Guid { get; set; } = Guid.NewGuid();

        [JsonIgnore]
        public bool IsSerializable { get; } = true;

        [JsonIgnore]
        internal static readonly ConcurrentNativeToManagedMapper mapper = new();

        [EditorProperty("Density")]
        public float Density
        { get => density; set { density = value; } }

        [EditorProperty("Static Friction")]
        public float StaticFriction
        { get => staticFriction; set { staticFriction = value; } }

        [EditorProperty("Dynamic Friction")]
        public float DynamicFriction
        { get => dynamicFriction; set { dynamicFriction = value; } }

        [EditorProperty("Restitution")]
        public float Restitution
        { get => restitution; set { restitution = value; } }

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        public void Awake()
        {
        }

        public void Destroy()
        {
        }

        public virtual void AddShapes(PxPhysics* physics, PxScene* scene, PxRigidActor* actor, Transform parent)
        {
            material = physics->CreateMaterialMut(staticFriction, dynamicFriction, restitution);

            Matrix4x4 relative;
            if (parent == GameObject.Transform)
            {
                relative = Matrix4x4.Identity;
            }
            else
            {
                relative = GameObject.Transform.GetRelativeTo(parent);
            }

            localPose = Helper.Convert(relative, out var scale);
            AddShapes(physics, scene, actor, localPose, scale);
        }

        public virtual void DestroyShapes()
        {
            for (int i = 0; i < shapes.Count; i++)
            {
                mapper.RemoveMapping(shapes[i]);
            }
            shapes.Clear();

            if (material != null)
            {
                ((PxBase*)material)->ReleaseMut();
                material = null;
            }
        }

        public abstract void AddShapes(PxPhysics* physics, PxScene* scene, PxRigidActor* actor, PxTransform localPose, Vector3 scale);

        public virtual void AttachShape(PxRigidActor* actor, PxShape* shape, PxTransform localPose)
        {
            shapes.Add(shape);
            mapper.AddMapping(shape, this);
            shape->SetLocalPoseMut(&localPose);
            actor->AttachShapeMut(shape);
            shape->ReleaseMut();
        }

        public virtual void AttachShape(PxRigidActor* actor, PxShape* shape)
        {
            shapes.Add(shape);
            mapper.AddMapping(shape, this);
            var pose = localPose;
            shape->SetLocalPoseMut(&pose);
            actor->AttachShapeMut(shape);
            shape->ReleaseMut();
        }

        public void BeginUpdate()
        {
        }

        public void EndUpdate()
        {
        }
    }
}