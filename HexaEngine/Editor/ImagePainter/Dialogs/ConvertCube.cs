namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Effects.Filter;
    using ImGuiNET;
    using Silk.NET.DirectStorage;

    public class ConvertCubeDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;
        private readonly IGraphicsDevice device;
        private CubemapType type;

        public ConvertCubeDialog(ImagePainterWindow imagePainter, IGraphicsDevice device)
        {
            this.imagePainter = imagePainter;
            this.device = device;
        }

        public override string Name => "Convert to cube";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

        public override void Reset()
        {
        }

        protected override void DrawContent()
        {
            ImGuiEnumHelper<CubemapType>.Combo("Type", ref type);
            if (ImGui.Button("Cancel"))
            {
                Close();
            }
            ImGui.SameLine();
            if (ImGui.Button("Convert"))
            {
                if (imagePainter.Source == null)
                    return;

                var image = imagePainter.Source.ToScratchImage(device);
                var meta = image.Metadata;
                IScratchImage converted;

                switch (type)
                {
                    case CubemapType.Cube:
                        {
                            meta.MiscFlags = TexMiscFlags.TextureCube;
                            converted = device.TextureLoader.Initialize(meta, CPFlags.None);
                            image.CopyTo(converted);
                            image.Dispose();
                        }
                        break;

                    case CubemapType.Panorama:
                        {
                            var srcTex = image.CreateTexture2D(device, Usage.Immutable, BindFlags.ShaderResource, CpuAccessFlags.None, ResourceMiscFlag.None);
                            var srv = device.CreateShaderResourceView(srcTex);

                            var dstTex = device.CreateTexture2D(meta.Format, meta.Width / 4, meta.Width / 4, 6, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.TextureCube);
                            var rtv = new RenderTargetViewArray(device, dstTex, 6, new(meta.Width / 4, meta.Width / 4));

                            EquiRectangularToCubeFilter filter = new(device);
                            filter.Source = srv;
                            filter.Targets = rtv;
                            filter.Draw(device.Context);

                            converted = device.TextureLoader.CaptureTexture(device.Context, dstTex);
                            srcTex.Dispose();
                            srv.Dispose();
                            dstTex.Dispose();
                            rtv.Dispose();
                            filter.Dispose();
                            image.Dispose();
                        }
                        break;

                    case CubemapType.Cross:
                        throw new NotSupportedException();
                    default:
                        throw new NotSupportedException();
                }

                imagePainter.Load(converted);

                converted.Dispose();

                Close();
            }
        }
    }
}