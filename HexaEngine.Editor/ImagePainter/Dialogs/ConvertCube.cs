namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Graphics.Filters;

    public class ConvertCubeDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;
        private CubemapType type;

        public ConvertCubeDialog(ImagePainterWindow imagePainter)
        {
            this.imagePainter = imagePainter;
        }

        public override string Name => "Convert to cube";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

        public override void Reset()
        {
        }

        protected override unsafe void DrawContent()
        {
            ComboEnumHelper<CubemapType>.Combo("Type", ref type);
            if (ImGui.Button("Cancel"))
            {
                Close();
            }
            ImGui.SameLine();
            if (ImGui.Button("Convert"))
            {
                if (imagePainter.Source == null)
                {
                    return;
                }

                var image = imagePainter.Source.ToScratchImage();
                var meta = image.Metadata;
                IScratchImage converted;
                var device = Application.GraphicsDevice;
                var context = device.Context;
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
                            Texture2D srcTex = new(new Texture2DDescription(Format.R32Float, 1, 1, 1, 1, GpuAccessFlags.Read), image);

                            Texture2D dstTex = new(new Texture2DDescription(meta.Format, meta.Width / 4, meta.Width / 4, 6, 1, GpuAccessFlags.RW, miscFlags: ResourceMiscFlag.TextureCube), image);
                            dstTex.CreateArraySlices();

                            EquiRectangularToCubeFilter filter = new(device);
                            filter.Source = srcTex.SRV;

                            filter.Draw(device.Context, dstTex.RTVArraySlices!, dstTex.Viewport);

                            converted = device.TextureLoader.CaptureTexture(device.Context, dstTex);
                            srcTex.Dispose();
                            dstTex.Dispose();
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