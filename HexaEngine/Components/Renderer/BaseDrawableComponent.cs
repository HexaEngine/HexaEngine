namespace HexaEngine.Components.Renderer
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Scenes;

    public abstract class BaseDrawableComponent : IDrawable
    {
        private volatile bool loaded = false;
        private uint queueIndex = (uint)RenderQueueIndex.Geometry;

        /// <summary>
        /// The GUID of the <see cref="BaseDrawableComponent"/>.
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
        public GameObject GameObject { get; set; } = null!;

        [JsonIgnore]
        public abstract RendererFlags Flags { get; }

        [JsonIgnore]
        public bool Loaded
        {
            get => loaded;
            protected set => loaded = value;
        }

        public bool BatchSupport { get; }

        int IDrawable.LeafId { get; set; } = -1;

        public event QueueIndexChangedEventHandler? QueueIndexChanged;

        public event DrawableInvalidatedEventHandler? DrawableInvalidated;

        protected abstract void LoadCore(IGraphicsDevice device);

        protected abstract void UnloadCore();

        public void Awake()
        {
            DebugName = GameObject.Name + DebugName;
        }

        public void Destroy()
        {
        }

        public void Invalidate()
        {
            DrawableInvalidated?.Invoke(this);
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
                GameObject.EnabledChanged += EnabledChanged;
                LoadCore(device);
                loaded = true;
            }
        }

        public void Unload()
        {
            if (loaded)
            {
                GameObject.EnabledChanged -= EnabledChanged;
                UnloadCore();
                loaded = false;
            }
        }

        private void EnabledChanged(GameObject sender, bool e)
        {
            Invalidate();
        }

        void IDrawable.Update(IGraphicsContext context)
        {
            if (loaded && GameObject.IsVisible)
            {
                Update(context);
            }
        }

        void IDrawable.VisibilityTest(CullingContext context)
        {
            if (loaded && GameObject.IsVisible)
            {
                VisibilityTest(context);
            }
        }

        void IDrawable.DrawDepth(IGraphicsContext context)
        {
            if (loaded && GameObject.IsVisible)
            {
                DrawDepth(context);
            }
        }

        void IDrawable.DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (loaded && GameObject.IsVisible)
            {
                DrawShadowMap(context, light, type);
            }
        }

        void IDrawable.Draw(IGraphicsContext context, RenderPath path)
        {
            if (loaded && GameObject.IsVisible)
            {
                Draw(context, path);
            }
        }

        void IDrawable.Bake(IGraphicsContext context)
        {
            if (loaded && GameObject.IsVisible)
            {
                Bake(context);
            }
        }

        public void Draw(IGraphicsContext deferred, string pass)
        {
            throw new NotImplementedException();
        }
    }
}