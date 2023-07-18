namespace HexaEngine.Lights.Probes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;

    [EditorComponent<IBLLightProbeComponent>("IBL Light Probe", false, true)]
    public class IBLLightProbeComponent : GlobalLightProbeComponent
    {
        private IGraphicsDevice device;
        private string diffuse = string.Empty;
        private string specular = string.Empty;

        [EditorProperty("Diffuse", null, "*.dds")]
        public string Diffuse
        {
            get => diffuse;
            set
            {
                diffuse = value;
                if (device != null)
                {
                    UpdateDiffuseAsync(device);
                }
            }
        }

        [EditorProperty("Specular", null, "*.dds")]
        public string Specular
        {
            get => specular;
            set
            {
                specular = value;
                if (device != null)
                {
                    UpdateSpecularAsync(device);
                }
            }
        }

        public override async void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.device = device;
            this.gameObject = gameObject;
            await UpdateDiffuseAsync(device);
            await UpdateSpecularAsync(device);
        }

        public override void Destroy()
        {
            Volatile.Write(ref isVaild, false);
            DiffuseTex?.Dispose();
            SpecularTex?.Dispose();
        }

        private Task UpdateDiffuseAsync(IGraphicsDevice device)
        {
            var state = new Tuple<IGraphicsDevice, IBLLightProbeComponent>(device, this);
            return Task.Factory.StartNew(async (state) =>
            {
                var p = (Tuple<IGraphicsDevice, IBLLightProbeComponent>)state;
                var device = p.Item1;
                var component = p.Item2;
                var path = Paths.CurrentAssetsPath + component.diffuse;

                Volatile.Write(ref component.isVaild, false);
                var old = component.diffuseTex;
                component.diffuseTex = null;
                old?.Dispose();

                if (FileSystem.Exists(path))
                {
                    try
                    {
                        component.diffuseTex = await Texture2D.CreateTextureAsync(device, new TextureFileDescription(path, TextureDimension.TextureCube));
                    }
                    catch (Exception ex)
                    {
                        ImGuiConsole.Log(ex);
                    }
                }

                Volatile.Write(ref component.isVaild, component.diffuseTex != null && component.specularTex != null);
            }, state);
        }

        private Task UpdateSpecularAsync(IGraphicsDevice device)
        {
            var state = new Tuple<IGraphicsDevice, IBLLightProbeComponent>(device, this);
            return Task.Factory.StartNew(async (state) =>
            {
                var p = (Tuple<IGraphicsDevice, IBLLightProbeComponent>)state;
                var device = p.Item1;
                var component = p.Item2;
                var path = Paths.CurrentAssetsPath + component.specular;

                Volatile.Write(ref component.isVaild, false);
                var old = component.specularTex;
                component.specularTex = null;
                old?.Dispose();

                if (FileSystem.Exists(path))
                {
                    try
                    {
                        component.specularTex = await Texture2D.CreateTextureAsync(device, new TextureFileDescription(path, TextureDimension.TextureCube));
                    }
                    catch (Exception ex)
                    {
                        ImGuiConsole.Log(ex);
                    }
                }

                Volatile.Write(ref component.isVaild, component.diffuseTex != null && component.specularTex != null);
            }, state);
        }
    }
}