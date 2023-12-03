namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;

    public abstract class BaseRendererComponent : IRendererComponent
    {
        private volatile bool loaded = false;
        private uint queueIndex = (uint)RenderQueueIndex.Geometry;

        [JsonIgnore]
        public uint QueueIndex
        {
            get => queueIndex;
            set
            {
                if (queueIndex == value)
                    return;
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

        public event QueueIndexChangedEventHandler? QueueIndexChanged;

        public abstract void Load(IGraphicsDevice device);

        public abstract void Unload();

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

        void IRendererComponent.Load(IGraphicsDevice device)
        {
            if (!loaded)
            {
                Load(device);
                loaded = true;
            }
        }

        void IRendererComponent.Unload()
        {
            if (loaded)
            {
                Unload();
                loaded = false;
            }
        }

        void IRendererComponent.Update(IGraphicsContext context)
        {
            if (loaded && GameObject.IsEnabled)
            {
                Update(context);
            }
        }

        void IRendererComponent.VisibilityTest(CullingContext context)
        {
            if (loaded && GameObject.IsEnabled)
            {
                VisibilityTest(context);
            }
        }

        void IRendererComponent.DrawDepth(IGraphicsContext context)
        {
            if (loaded && GameObject.IsEnabled)
            {
                DrawDepth(context);
            }
        }

        void IRendererComponent.DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (loaded && GameObject.IsEnabled)
            {
                DrawShadowMap(context, light, type);
            }
        }

        void IRendererComponent.Draw(IGraphicsContext context, RenderPath path)
        {
            if (loaded && GameObject.IsEnabled)
            {
                Draw(context, path);
            }
        }

        void IRendererComponent.Bake(IGraphicsContext context)
        {
            if (loaded && GameObject.IsEnabled)
            {
                Bake(context);
            }
        }
    }
}