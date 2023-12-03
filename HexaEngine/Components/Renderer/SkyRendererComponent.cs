namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Weather;
    using System;
    using System.Threading.Tasks;

    [EditorComponent<SkyRendererComponent>("Sky", false, true)]
    public class SkyRendererComponent : BaseRendererComponent
    {
        private SkyRenderer renderer;
        private Skybox skybox;

        private IGraphicsDevice? device;
        private string environmentPath = string.Empty;
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

        [EditorProperty("Env", null)]
        public string Environment
        {
            get => environmentPath;
            set
            {
                environmentPath = value;
                if (device == null)
                {
                    return;
                }

                UpdateEnvAsync(device);
            }
        }

        public override void Load(IGraphicsDevice device)
        {
            renderer = new(device);

            UpdateEnvAsync(device).Wait();
        }

        public override void Unload()
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

        private Task UpdateEnvAsync(IGraphicsDevice device)
        {
            Loaded = false;
            renderer?.Uninitialize();
            var tmpSkybox = skybox;
            skybox = null;
            tmpSkybox?.Dispose();

            var state = new Tuple<IGraphicsDevice, SkyRendererComponent>(device, this);
            return Task.Factory.StartNew(async (state) =>
            {
                var p = (Tuple<IGraphicsDevice, SkyRendererComponent>)state;
                var device = p.Item1;
                var component = p.Item2;
                var path = Paths.CurrentAssetsPath + component.environmentPath;

                if (FileSystem.Exists(path))
                {
                    component.skybox = new(device);
                    await component.skybox.LoadAsync(path);
                    component.renderer.Initialize(component.skybox);
                    component.Loaded = true;
                }
            }, state);
        }
    }
}