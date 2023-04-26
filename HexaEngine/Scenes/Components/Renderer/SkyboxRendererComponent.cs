namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Rendering;
    using System;
    using System.Threading.Tasks;

    [EditorComponent<SkyboxRendererComponent>("Skybox", false, true)]
    public class SkyboxRendererComponent : IRenderComponent
    {
        private GameObject gameObject;
        private IGraphicsDevice? device;
        private SkyboxRenderer renderer;
        private Texture? env;
        private string environment = string.Empty;
        private bool drawable;

        private SkyboxData data;
        private Pointer<ObjectHandle> handle;

        [EditorProperty("Env", null)]
        public string Environment
        {
            get => environment;
            set
            {
                environment = value;
                if (device == null)
                {
                    return;
                }

                Volatile.Write(ref drawable, false);
                UpdateEnvAsync(device);
            }
        }

        public async void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.device = device;
            if (!gameObject.GetScene().TryGetSystem<RenderManager>(out var manager))
            {
                return;
            }

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
            handle = default;
        }

        public void Draw()
        {
            if (!Volatile.Read(ref drawable) || !gameObject.IsEnabled)
            {
                return;
            }

            renderer.Draw(handle);
        }

        private Task UpdateEnvAsync(IGraphicsDevice device)
        {
            var state = new Tuple<IGraphicsDevice, SkyboxRendererComponent>(device, this);
            return Task.Factory.StartNew(async (state) =>
            {
                var p = (Tuple<IGraphicsDevice, SkyboxRendererComponent>)state;
                var device = p.Item1;
                var component = p.Item2;
                var path = Paths.CurrentAssetsPath + component.environment;

                unsafe
                {
                    // Inform renderer to stop render the skybox.
                    Volatile.Write(ref component.drawable, false);
                    component.data.Enviornment = null;
                    component.renderer.UpdateInstance(component.handle, component.data);
                }

                component.env?.Dispose();
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
                Volatile.Write(ref component.drawable, true);
            }, state);
        }
    }
}