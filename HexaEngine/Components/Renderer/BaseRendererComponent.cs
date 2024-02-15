namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;

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

        public bool BatchSupport { get; }

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

        public static MaterialData GetMaterial(AssetRef assetRef)
        {
            if (assetRef == AssetRef.Empty)
            {
                return MaterialData.Empty;
            }
            else
            {
                Artifact? artifact = ArtifactDatabase.GetArtifact(assetRef);
                if (artifact == null)
                {
                    Logger.Warn($"Failed to load material {assetRef}");
                    return MaterialData.Empty;
                }
                if (artifact.Type != AssetType.Material)
                {
                    Logger.Warn($"Failed to load material {assetRef}, asset was {artifact.Type} but needs to be {AssetType.Material}");
                    return MaterialData.Empty;
                }

                Stream? stream = null;

                try
                {
                    stream = artifact.OpenRead();
                    MaterialFile materialFile = MaterialFile.Read(stream);
                    return materialFile;
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                    Logger.Warn($"Failed to load material {assetRef}");
                    return MaterialData.Empty;
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }
    }
}