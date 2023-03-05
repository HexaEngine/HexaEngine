namespace HexaEngine.Core.Renderers.Components
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Core.Unsafes;
    using System;
    using System.Reflection.Metadata;
    using System.Threading.Tasks;

    [EditorComponent<SkyboxRendererComponent>("Skybox", false, true)]
    public class SkyboxRendererComponent : IRenderComponent
    {
        private IGraphicsDevice? device;
        private SkyboxRenderer renderer;
        private Texture? env;
        private string environment = string.Empty;
        private bool drawable;

        private SkyboxData data;
        private Pointer<ObjectHandle> handle;

        [EditorProperty("Environment")]
        public string Environment
        {
            get => environment;
            set
            {
                environment = value;
                if (device == null)
                    return;
                Volatile.Write(ref drawable, false);
                UpdateEnvAsync(device);
            }
        }

        public async void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.device = device;
            if (!gameObject.GetScene().TryGetSystem<RenderManager>(out var manager))
                return;
            renderer = manager.GetRenderer<SkyboxRenderer>();

            data = new(null);
            handle = renderer.CreateInstance(data);
            await UpdateEnvAsync(device);
        }

        public unsafe void Destory()
        {
            Volatile.Write(ref drawable, false);
            renderer.DestroyInstance(handle);
            env?.Dispose();
            handle.Free();
        }

        public void Draw()
        {
            if (!Volatile.Read(ref drawable))
                return;
            renderer.Draw(handle);
        }

        private Task UpdateEnvAsync(IGraphicsDevice device)
        {
            var state = (device, this);
            return Task.Factory.StartNew(async (object? state) =>
            {
                var p = ((IGraphicsDevice, SkyboxRendererComponent))state;
                var device = p.Item1;
                var component = p.Item2;
                var path = Paths.CurrentTexturePath + component.environment;

                if (FileSystem.Exists(path))
                {
                    try
                    {
                        component.env = await Texture.CreateTextureAsync(device, new TextureFileDescription(path, TextureDimension.TextureCube));
                    }
                    catch (Exception ex)
                    {
                        ImGuiConsole.Log(ex);
                        component.env = await Texture.CreateTextureAsync(device, TextureDimension.TextureCube, default);
                    }
                }
                else
                {
                    component.env = await Texture.CreateTextureAsync(device, TextureDimension.TextureCube, default);
                }
                unsafe
                {
                    component.data.Enviornment = (void*)(component.env.ShaderResourceView?.NativePointer ?? 0);
                    component.renderer.UpdateInstance(component.handle, component.data);
                }
                Volatile.Write(ref drawable, true);
            }, state);
        }
    }
}