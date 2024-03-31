namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Jobs;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Weather;
    using System;

    [EditorCategory("Renderer")]
    [EditorComponent<SkyRendererComponent>("Sky", false, true)]
    public class SkyRendererComponent : BaseRendererComponent
    {
        private SkyRenderer renderer;
        private Skybox skybox;

        private IGraphicsDevice? device;
        private AssetRef environmentPath;
        private SkyType skyType;

        public SkyRendererComponent()
        {
            QueueIndex = (uint)RenderQueueIndex.Background;
        }

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(SkyRenderer);

        [JsonIgnore]
        public override RendererFlags Flags { get; } = RendererFlags.Update | RendererFlags.Depth | RendererFlags.Draw;

        [JsonIgnore]
        public override BoundingBox BoundingBox { get; }

        [EditorProperty<SkyType>("Type")]
        public SkyType SkyType { get => skyType; set => skyType = value; }

        [EditorProperty("Environment", AssetType.TextureCube)]
        public AssetRef Environment
        {
            get => environmentPath;
            set
            {
                environmentPath = value;
                if (device == null)
                {
                    return;
                }

                UpdateEnvAsync();
            }
        }

        protected override void LoadCore(IGraphicsDevice device)
        {
            renderer = new(device);
            UpdateEnvAsync();
        }

        protected override void UnloadCore()
        {
            renderer.Dispose();
            skybox?.Dispose();
        }

        public override void Update(IGraphicsContext context)
        {
            renderer.Update(context);
        }

        public override void DrawDepth(IGraphicsContext context)
        {
            throw new NotSupportedException();
        }

        public override void VisibilityTest(CullingContext context)
        {
            throw new NotSupportedException();
        }

        public override void Draw(IGraphicsContext context, RenderPath path)
        {
            if (path == RenderPath.Forward)
            {
                renderer.Draw(context, skyType);
            }
        }

        public override void Bake(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public override void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            throw new NotSupportedException();
        }

        private Job UpdateEnvAsync()
        {
            Loaded = false;
            renderer?.Uninitialize();
            var tmpSkybox = skybox;
            skybox = null;
            tmpSkybox?.Dispose();

            return Job.Run("Sky Load Job", this, state =>
            {
                if (state is not SkyRendererComponent component)
                {
                    return;
                }

                if (component.device == null)
                {
                    return;
                }

                var device = component.device;

                var path = component.environmentPath.GetPath();

                if (path != null && File.Exists(path))
                {
                    component.skybox = new(device);
                    component.skybox.LoadAsync(path).Wait();
                    component.renderer.Initialize(component.skybox);
                    component.Loaded = true;
                }
            }, JobPriority.Normal, JobFlags.BlockOnSceneLoad);
        }
    }
}