namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Animations;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;

    public abstract class BaseRendererComponent : IRendererComponent
    {
        private volatile bool loaded = false;
        private uint queueIndex = (uint)RenderQueueIndex.Geometry;

        /// <summary>
        /// The GUID of the <see cref="BaseRendererComponent"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        public Guid Guid { get; set; } = Guid.NewGuid();

        [JsonIgnore]
        public bool IsSerializable { get; } = true;

        [JsonIgnore]
        public uint QueueIndex
        {
            get => queueIndex;
            set
            {
                if (queueIndex == value)
                {
                    return;
                }

                uint old = queueIndex;
                queueIndex = value;
                QueueIndexChanged?.Invoke(this, old, value);
            }
        }

        [JsonIgnore]
        public abstract BoundingBox BoundingBox { get; }

        [JsonIgnore]
        public abstract string DebugName { get; protected set; }

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        [JsonIgnore]
        public abstract RendererFlags Flags { get; }

        [JsonIgnore]
        public bool Loaded
        {
            get => loaded;
            protected set => loaded = value;
        }

        public bool BatchSupport { get; }

        public event QueueIndexChangedEventHandler? QueueIndexChanged;

        protected abstract void LoadCore(IGraphicsDevice device);

        protected abstract void UnloadCore();

        public void Awake()
        {
            DebugName = GameObject.Name + DebugName;
        }

        public void Destroy()
        {
        }

        public abstract void Draw(IGraphicsContext context, RenderPath path);

        public abstract void DrawDepth(IGraphicsContext context);

        public abstract void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type);

        public abstract void Bake(IGraphicsContext context);

        public abstract void Update(IGraphicsContext context);

        public abstract void VisibilityTest(CullingContext context);

        public void Load(IGraphicsDevice device)
        {
            if (!loaded)
            {
                LoadCore(device);
                loaded = true;
            }
        }

        public void Unload()
        {
            if (loaded)
            {
                UnloadCore();
                loaded = false;
            }
        }

        void IRendererComponent.Update(IGraphicsContext context)
        {
            if (loaded && GameObject.IsVisible)
            {
                Update(context);
            }
        }

        void IRendererComponent.VisibilityTest(CullingContext context)
        {
            if (loaded && GameObject.IsVisible)
            {
                VisibilityTest(context);
            }
        }

        void IRendererComponent.DrawDepth(IGraphicsContext context)
        {
            if (loaded && GameObject.IsVisible)
            {
                DrawDepth(context);
            }
        }

        void IRendererComponent.DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (loaded && GameObject.IsVisible)
            {
                DrawShadowMap(context, light, type);
            }
        }

        void IRendererComponent.Draw(IGraphicsContext context, RenderPath path)
        {
            if (loaded && GameObject.IsVisible)
            {
                Draw(context, path);
            }
        }

        void IRendererComponent.Bake(IGraphicsContext context)
        {
            if (loaded && GameObject.IsVisible)
            {
                Bake(context);
            }
        }
    }
}