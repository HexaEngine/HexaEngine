namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering;
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;

    [EditorComponent<SkyboxRendererComponent>("Skybox", false, true)]
    public class SkyboxRendererComponent : IRendererComponent
    {
        private GameObject gameObject;
        private SkyRenderer renderer;
        private Skybox skybox;

        private IGraphicsDevice? device;
        private string environmentPath = string.Empty;

        [JsonIgnore]
        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Background;

        [JsonIgnore]
        public RendererFlags Flags { get; } = RendererFlags.Update | RendererFlags.Depth | RendererFlags.Draw;

        [JsonIgnore]
        public BoundingBox BoundingBox { get; }

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

        public unsafe void Destory()
        {
            renderer.Dispose();
            skybox.Dispose();
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

        public void Draw(IGraphicsContext context)
        {
            if (!gameObject.IsEnabled)
                return;
            renderer.Draw(context, SkyType.HosekWilkie);
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

            var state = new Tuple<IGraphicsDevice, SkyboxRendererComponent>(device, this);
            return Task.Factory.StartNew(async (state) =>
            {
                var p = (Tuple<IGraphicsDevice, SkyboxRendererComponent>)state;
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