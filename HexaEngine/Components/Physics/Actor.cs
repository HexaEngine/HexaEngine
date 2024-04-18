namespace HexaEngine.Components.Physics
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Physics;
    using HexaEngine.Scenes;
    using MagicPhysX;
    using System;
    using System.Runtime.InteropServices;

    // TODO: Constraints.
    // TODO: Dominance Group Editor.
    public abstract unsafe class Actor : IActorComponent
    {
        protected PxScene* scene;
        private PxActor* actor;
        private byte* name;
        protected ActorType type;
        protected bool isUpdating;
        private bool disableGravity;
        private bool sendSleepNotifies;
        private byte dominanceGroup = 0;

        [JsonIgnore]
        internal static readonly ConcurrentNativeToManagedMapper mapper = new();

        /// <summary>
        /// The GUID of the <see cref="Actor"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        public Guid Guid { get; set; } = Guid.NewGuid();

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        [EditorProperty<ActorType>("Type")]
        public ActorType Type
        { get => type; set { type = value; NotifyOnRecreate(); } }

        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Disable Gravity")]
        public bool DisableGravity
        {
            get => disableGravity;
            set
            {
                disableGravity = value;

                if (actor != null)
                {
                    actor->SetActorFlagMut(PxActorFlag.DisableGravity, value);
                }
            }
        }

        [EditorPropertyCondition<RigidBody>(nameof(IsDynamic))]
        [EditorProperty("Send Sleep Notifies")]
        public bool SendSleepNotifies
        {
            get => sendSleepNotifies;
            set
            {
                sendSleepNotifies = value;

                if (actor != null)
                {
                    actor->SetActorFlagMut(PxActorFlag.SendSleepNotifies, value);
                }
            }
        }

        [EditorProperty("Dominance Group")]
        public byte DominanceGroup
        {
            get
            {
                if (actor != null)
                {
                    dominanceGroup = actor->GetDominanceGroup();
                }

                return dominanceGroup;
            }
            set
            {
                dominanceGroup = value;

                if (actor != null)
                {
                    actor->SetDominanceGroupMut(value);
                }
            }
        }

        [JsonIgnore]
        public ActorFlags Flags
        {
            get
            {
                ActorFlags flags = 0;
                if (PhysicsSystem.Debug)
                {
                    flags |= ActorFlags.Visualization;
                }

                if (disableGravity)
                {
                    flags |= ActorFlags.DisableGravity;
                }

                if (sendSleepNotifies)
                {
                    flags |= ActorFlags.SendSleepNotifies;
                }

                if (!GameObject.IsEnabled)
                {
                    flags |= ActorFlags.DisableSimulation;
                }

                return flags;
            }
        }

        public event Action<IActorComponent>? OnRecreate;

        protected static bool IsStatic(RigidBody rigidBody)
        {
            return rigidBody.type == ActorType.Static;
        }

        protected static bool IsDynamic(RigidBody rigidBody)
        {
            return rigidBody.type != ActorType.Static;
        }

        protected void NotifyOnRecreate()
        {
            isUpdating = true;
            OnRecreate?.Invoke(this);
        }

        protected abstract PxActor* CreateActor(PxPhysics* physics, PxScene* scene);

        void IActorComponent.CreateActor(PxPhysics* physics, PxScene* scene)
        {
            this.scene = scene;
            actor = CreateActor(physics, scene);
            if (actor != null)
            {
                mapper.AddMapping(actor, this);

                actor->SetActorFlagsMut(Helper.Convert(Flags));
                actor->SetDominanceGroupMut(dominanceGroup);

                name = GameObject.FullName.ToUTF8Ptr();
                actor->SetNameMut(name);

                bool wasAdded = scene->AddActorMut(actor, null);
                if (!wasAdded)
                {
                    PhysicsSystem.Logger.Error($"{GameObject.FullName}: Actor couldn't be added");
                }
            }
            isUpdating = false;
        }

        protected virtual void DestroyActor()
        {
            scene = null;
            if (actor != null)
            {
                mapper.RemoveMapping(actor);
                var scene = actor->GetScene();
                scene->RemoveActorMut(actor, true);
                actor->SetNameMut(null);
                actor->ReleaseMut();
                actor = null;
            }

            if (name != null)
            {
                Free(name);
                name = null;
            }
        }

        void IActorComponent.DestroyActor()
        {
            DestroyActor();
        }

        protected abstract void BeginUpdate();

        protected abstract void EndUpdate();

        void IActorComponent.BeginUpdate()
        {
            BeginUpdate();
        }

        void IActorComponent.EndUpdate()
        {
            EndUpdate();
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Destroy()
        {
        }

        void IComponent.Awake()
        {
            GameObject.EnabledChanged += OnEnabledChanged;
            GameObject.NameChanged += OnNameChanged;
            Awake();
        }

        void IComponent.Destroy()
        {
            GameObject.EnabledChanged -= OnEnabledChanged;
            GameObject.NameChanged -= OnNameChanged;
            Destroy();
        }

        private void OnEnabledChanged(GameObject gameObject, bool enabled)
        {
            if (actor != null)
            {
                actor->SetActorFlagMut(PxActorFlag.DisableSimulation, !enabled);
            }
        }

        private void OnNameChanged(GameObject gameObject, string name)
        {
            if (actor != null)
            {
                byte* newName = gameObject.FullName.ToUTF8Ptr();
                actor->SetNameMut(newName);
                Free(this.name);
                this.name = newName;
            }
        }

        public BoundingBox GetWorldBounds(float inflation = 1.01f)
        {
            PxBounds3 bounds = actor->GetWorldBounds(inflation);
            BoundingBox boundingBox = new(bounds.minimum, bounds.maximum);
            return boundingBox;
        }
    }
}