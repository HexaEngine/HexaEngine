namespace HexaEngine.Components.Collider
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Physics;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Systems;
    using MagicPhysX;
    using System.Numerics;

    public abstract unsafe class BaseCollider : IPhysXColliderComponent
    {
        protected bool hasActor = false;
        protected bool hasShape = false;
        protected bool inCompound = false;
        protected bool update = true;

        protected Scene? scene;
        protected PhysicsSystem? system;
        protected PxScene* pxScene;
        protected PxPhysics* pxPhysics;
        protected object syncObject = new();
        protected ColliderType type;

        protected PxRigidActor* actor;
        protected PxShape* shape;
        protected PxTransform pose;
        protected PxMaterial* material;

        private float density = 1;
        private float dynamicFriction = 0.5f;
        private float staticFriction = 0.5f;
        private float restitution = 0.6f;

        [EditorProperty<ColliderType>("Type")]
        public ColliderType Type
        { get => type; set { type = value; update = true; } }

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
        public bool HasActor => hasActor;

        [JsonIgnore]
        public bool HasShape => hasShape;

        [JsonIgnore]
        public bool InCompound => inCompound;

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        public void Awake()
        {
            scene = GameObject.GetScene();
            system = scene.GetRequiredSystem<PhysicsSystem>();
            pxScene = system.PxScene;
            pxPhysics = PhysicsSystem.PxPhysics;
            syncObject = system.SyncObject;
            CreateActor();
        }

        public void Destroy()
        {
            DestroyActor();
        }

        public void CreateActor()
        {
            if (Application.InDesignMode || GameObject == null || pxScene == null || inCompound || hasActor)
            {
                return;
            }

            material = NativeMethods.PxPhysics_createMaterial_mut(pxPhysics, staticFriction, dynamicFriction, restitution);

            Logger.Assert(material != null);

            DestroyActor();
            CreateShape();

            var transform = PhysXHelper.Convert(GameObject.Transform.GlobalPosition, GameObject.Transform.GlobalOrientation); ;
            var offset = NativeMethods.PxTransform_new_2(PxIDENTITY.PxIdentity);

            lock (syncObject)
            {
                if (Type == ColliderType.Static)
                {
                    actor = (PxRigidActor*)NativeMethods.PxPhysics_createRigidStatic_mut(pxPhysics, &transform);
                }
                if (Type == ColliderType.Dynamic)
                {
                    actor = (PxRigidActor*)NativeMethods.PxPhysics_createRigidDynamic_mut(pxPhysics, &transform);
                }
                if (Type == ColliderType.Kinematic)
                {
                    //actor = (PxRigidActor*)NativeMethods.PxPhysics_createRi(pxPhysics, &transform);
                }

                if (shape != null)
                {
                    bool wasAttached = actor->AttachShapeMut(shape);

                    shape->ReleaseMut();

                    if (!wasAttached)
                    {
                        Logger.Error($"{GameObject.FullName}: Shape couldn't be attached to actor");
                    }
                }
                else
                {
                    AddShapes(actor);
                }

                Logger.Assert(actor != null, "Actor was null");

                bool wasAdded = NativeMethods.PxScene_addActor_mut(pxScene, (PxActor*)actor, null);
                if (!wasAdded)
                {
                    Logger.Error($"{GameObject.FullName}: Actor couldn't be added");
                }
            }

            hasActor = true;
            update = false;
        }

        public void DestroyActor()
        {
            if (Application.InDesignMode || GameObject == null || pxScene == null || !hasActor)
            {
                return;
            }

            hasActor = false;

            actor->ReleaseMut();
        }

        public abstract void CreateShape();

        public virtual void AddShapes(PxRigidActor* actor)
        {
        }

        public void BeginUpdate()
        {
            if (update)
            {
                CreateActor();
            }

            if (type != ColliderType.Static && GameObject != null)
            {
                PxTransform transform = PhysXHelper.Convert(GameObject.Transform.PositionRotation);

                //actor->SetGlobalPoseMut(&transform, true);
            }
        }

        public void EndUpdate()
        {
            if (update)
            {
                CreateActor();
            }

            if (type != ColliderType.Static && GameObject != null)
            {
                var pose = actor->GetGlobalPose();
                GameObject.Transform.PositionRotation = PhysXHelper.Convert(pose);
            }
        }
    }
}