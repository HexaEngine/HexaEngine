namespace HexaEngine.Components.Physics.Collider
{
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Physics;
    using MagicPhysX;
    using System.Numerics;

    public abstract unsafe class BaseCollider : IColliderComponent
    {
        protected PxMaterial* material;

        private float density = 1;
        private float dynamicFriction = 0.5f;
        private float staticFriction = 0.5f;
        private float restitution = 0.6f;
        private PxTransform localPose;

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
            if (material == null)
            {
                ((PxBase*)material)->ReleaseMut();
                material = null;
            }
        }

        public abstract void AddShapes(PxPhysics* physics, PxScene* scene, PxRigidActor* actor, PxTransform localPose, Vector3 scale);

        public virtual void AttachShape(PxRigidActor* actor, PxShape* shape, PxTransform localPose)
        {
            shape->SetLocalPoseMut(&localPose);
            actor->AttachShapeMut(shape);
            shape->ReleaseMut();
        }

        public virtual void AttachShape(PxRigidActor* actor, PxShape* shape)
        {
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