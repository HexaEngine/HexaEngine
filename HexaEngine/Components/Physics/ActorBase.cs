namespace HexaEngine.Components.Physics
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Physics;
    using MagicPhysX;
    using System;

    // TODO: Constraints.
    // TODO: Dominance Group Editor.
    public abstract unsafe class ActorBase : IActorComponent
    {
        private PxActor* actor;
        private byte* name;
        protected ActorType type;
        protected bool isUpdating;
        private bool disableGravity;
        private bool sendSleepNotifies;
        private byte dominanceGroup = 0;

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        [EditorProperty<ActorType>("Type")]
        public ActorType Type
        { get => type; set { type = value; NotifyOnRecreate(); } }

        [EditorPropertyCondition<RigidBody>(nameof(IsStatic))]
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

        [EditorPropertyCondition<RigidBody>(nameof(IsStatic))]
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

        protected void NotifyOnRecreate()
        {
            isUpdating = true;
            OnRecreate?.Invoke(this);
        }

        public abstract PxActor* CreateActor(PxPhysics* physics, PxScene* scene);

        void IActorComponent.CreateActor(PxPhysics* physics, PxScene* scene)
        {
            actor = CreateActor(physics, scene);
            if (actor != null)
            {
                actor->SetActorFlagsMut(Helper.Convert(Flags));
                actor->SetDominanceGroupMut(dominanceGroup);

                name = GameObject.FullName.ToUTF8Ptr();
                actor->SetNameMut(name);

                bool wasAdded = scene->AddActorMut(actor, null);
                if (!wasAdded)
                {
                    Logger.Error($"{GameObject.FullName}: Actor couldn't be added");
                }
            }
            isUpdating = false;
        }

        public virtual void DestroyActor()
        {
            if (actor != null)
            {
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

        public abstract void BeginUpdate();

        public abstract void EndUpdate();

        public virtual void Awake()
        {
            GameObject.OnEnabledChanged += OnEnabledChanged;
            GameObject.OnNameChanged += OnNameChanged;
        }

        public virtual void Destroy()
        {
            GameObject.OnEnabledChanged -= OnEnabledChanged;
            GameObject.OnNameChanged -= OnNameChanged;
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