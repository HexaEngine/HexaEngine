namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.Renderers;
    using HexaEngine.Scenes;
    using System;
    using System.Threading.Tasks;

    [EditorComponent<SkyRendererComponent>("Sky", false, true)]
    public class SkyRendererComponent : IRendererComponent
    {
        private GameObject gameObject;
        private SkyRenderer renderer;
        private Skybox skybox;

        private IGraphicsDevice? device;
        private string environmentPath = string.Empty;
        private SkyType skyType;

        [JsonIgnore]
        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Background;

        [JsonIgnore]
        public RendererFlags Flags { get; } = RendererFlags.Update | RendererFlags.Depth | RendererFlags.Draw;

        [JsonIgnore]
        public BoundingBox BoundingBox { get; }

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

        public async void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.device = device;
            renderer = new(device);

            await UpdateEnvAsync(device);
        }

        public unsafe void Destroy()
        {
            renderer.Dispose();
            skybox?.Dispose();
        }

        public void Update(IGraphicsContext context)
        {
            if (!gameObject.IsEnabled)
                return;
            renderer.Update(context);
        }

        public void DrawDepth(IGraphicsContext context)
        {
            throw new NotSupportedException();
        }

        public void VisibilityTest(IGraphicsContext context, Camera camera)
        {
            throw new NotSupportedException();
        }

        public void Draw(IGraphicsContext context, RenderPath path)
        {
            if (!gameObject.IsEnabled)
                return;
            renderer.Draw(context, skyType);
        }

        public void Bake(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void DrawIndirect(IGraphicsContext context, IBuffer argsBuffer, int offset)
        {
            throw new NotImplementedException();
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            throw new NotSupportedException();
        }

        private Task UpdateEnvAsync(IGraphicsDevice device)
        {
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
                    await component.skybox.LoadAsync(environmentPath);
                    component.renderer.Initialize(component.skybox);
                }
            }, state);
        }
    }
}