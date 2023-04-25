namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Effects.Filter;
    using ImGuiNET;

    public class IrradianceFilterDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;
        private readonly IGraphicsDevice device;

        private const int irrTileSize = 256;
        private int irrSize = 0;

        private ISamplerState? samplerState;
        private ITexture2D? srcTex;
        private IShaderResourceView? srcSrv;
        private ITexture2D? dstTex;
        private IShaderResourceView? dstSrv;
        private RenderTargetViewArray? irrRTV;
        private ShaderResourceViewArray? irrSRV;
        private IrradianceFilter? irradianceFilter;

        private bool compute;
        private int side;
        private int x;
        private int y;
        private int step;
        private int steps;

        public override string Name => "Bake Irradiance";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

        public IrradianceFilterDialog(ImagePainterWindow imagePainter, IGraphicsDevice device)
        {
            this.imagePainter = imagePainter;
            this.device = device;
        }

        private void Discard()
        {
            irrRTV?.Dispose();
            irrRTV = null;
            irrSRV?.Dispose();
            irrSRV = null;

            srcTex?.Dispose();
            srcTex = null;
            srcSrv?.Dispose();
            srcSrv = null;
            dstTex?.Dispose();
            dstTex = null;
            dstSrv?.Dispose();
            dstSrv = null;

            samplerState?.Dispose();
            irradianceFilter?.Dispose();
            srcTex?.Dispose();
        }

        protected override void DrawContent()
        {
            if (compute)
                ImGui.BeginDisabled(true);

            if (ImGui.Button("Compute"))
            {
                if (imagePainter.Source != null)
                {
                    try
                    {
                        Discard();

                        samplerState = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
                        irradianceFilter = new(device);

                        var image = imagePainter.Source.ToScratchImage(device);

                        srcTex = image.CreateTexture2D(device, Usage.Immutable, BindFlags.ShaderResource, CpuAccessFlags.None, ResourceMiscFlag.TextureCube);
                        srcSrv = device.CreateShaderResourceView(srcTex);

                        var desc = srcTex.Description;
                        desc.Usage = Usage.Default;
                        desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
                        desc.MipLevels = 1;

                        dstTex = device.CreateTexture2D(desc);
                        dstSrv = device.CreateShaderResourceView(dstTex);

                        irrRTV = new(device, dstTex, 6, new(desc.Width, desc.Height));
                        irrSRV = new(device, dstTex, 6);

                        irradianceFilter.Targets = irrRTV;
                        irradianceFilter.Source = srcSrv;

                        irrSize = desc.Width;
                        steps = 6 * (irrSize / irrTileSize) * (irrSize / irrTileSize);

                        compute = true;
                        ImGui.BeginDisabled(true);
                    }
                    catch (Exception e)
                    {
                        ImGuiConsole.Log(e);
                    }
                }
            }

            if (dstTex != null && irrRTV != null && irrSRV != null)
            {
                ImGui.SameLine();

                if (ImGui.Button("Apply"))
                {
                    var loader = device.TextureLoader;
                    var image = loader.CaptureTexture(device.Context, dstTex);
                    imagePainter.Load(image);
                    image.Dispose();
                    Discard();
                    Close();
                    return;
                }

                ImGui.SameLine();

                if (ImGui.Button("Cancel"))
                {
                    Discard();
                    Close();
                    return;
                }

                if (compute)
                {
                    ImGui.EndDisabled();
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        compute = false;
                        side = 0;
                        x = 0;
                        y = 0;
                        step = 0;
                        Discard();
                        Close();
                        return;
                    }

                    ImGui.SameLine();

                    ImGui.TextColored(new(0, 1, 0, 1), $"Filtering... {(float)step / steps * 100f:n2} ({step} / {steps})");
                }

                for (int i = 0; i < 6; i++)
                {
                    ImGui.Image(irrSRV.Views[i].NativePointer, new(128, 128));
                    if (i != 5)
                        ImGui.SameLine();
                }
            }

            if (compute)
            {
                if (side < 6)
                {
                    if (x < irrSize)
                    {
                        if (y < irrSize)
                        {
                            irradianceFilter.DrawSlice(device.Context, side, x, y, irrTileSize, irrTileSize);
                            y += irrTileSize;
                            step++;
                        }
                        else
                        {
                            x += irrTileSize;
                            y = 0;
                        }
                    }
                    else
                    {
                        side++;
                        x = 0;
                        y = 0;
                    }
                }
                else
                {
                    compute = false;
                    side = 0;
                    x = 0;
                    y = 0;
                    step = 0;
                    srcTex?.Dispose();
                    srcTex = null;
                }
            }
        }

        public override void Reset()
        {
        }
    }
}