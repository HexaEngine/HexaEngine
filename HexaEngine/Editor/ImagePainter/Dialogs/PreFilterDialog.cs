namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Effects.Filter;
    using ImGuiNET;
    using System;

    public class PreFilterDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;
        private readonly IGraphicsDevice device;

        private const int pfTileSize = 1024;
        private int pfSize = 0;

        private ISamplerState? samplerState;
        private ITexture2D? srcTex;
        private IShaderResourceView? srcSrv;
        private ITexture2D? dstTex;
        private IShaderResourceView? dstSrv;
        private RenderTargetViewArray? pfRTV;
        private ShaderResourceViewArray? pfSRV;

        private PreFilter? prefilterFilter;
        private float roughness;

        private bool compute;
        private int side;
        private int x;
        private int y;
        private int step;
        private int steps;

        public override string Name => "IBL Prefilter";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

        public PreFilterDialog(ImagePainterWindow imagePainter, IGraphicsDevice device)
        {
            this.imagePainter = imagePainter;
            this.device = device;
        }

        private void Discard()
        {
            pfRTV?.Dispose();
            pfRTV = null;
            pfSRV?.Dispose();
            pfSRV = null;

            srcTex?.Dispose();
            srcTex = null;
            srcSrv?.Dispose();
            srcSrv = null;
            dstTex?.Dispose();
            dstTex = null;
            dstSrv?.Dispose();
            dstSrv = null;

            samplerState?.Dispose();
            prefilterFilter?.Dispose();
            srcTex?.Dispose();
        }

        protected override void DrawContent()
        {
            if (compute)
                ImGui.BeginDisabled(true);

            if (ImGui.SliderFloat("Roughness", ref roughness, 0, 1))
            {
            }

            if (ImGui.Button("Filter") && imagePainter.Source != null)
            {
                try
                {
                    Discard();

                    samplerState = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
                    prefilterFilter = new(device);
                    prefilterFilter.Roughness = roughness;

                    var image = imagePainter.Source.ToScratchImage(device);

                    srcTex = image.CreateTexture2D(device, Usage.Immutable, BindFlags.ShaderResource, CpuAccessFlags.None, ResourceMiscFlag.TextureCube);
                    srcSrv = device.CreateShaderResourceView(srcTex);

                    var desc = srcTex.Description;
                    desc.Usage = Usage.Default;
                    desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
                    desc.MipLevels = 1;

                    dstTex = device.CreateTexture2D(desc);
                    dstSrv = device.CreateShaderResourceView(dstTex);

                    pfRTV = new(device, dstTex, 6, new(desc.Width, desc.Height));
                    pfSRV = new(device, dstTex, 6);

                    prefilterFilter.Targets = pfRTV;
                    prefilterFilter.Source = srcSrv;

                    pfSize = desc.Width;
                    steps = 6 * (desc.Width / pfTileSize) * (desc.Width / pfTileSize);

                    compute = true;
                    ImGui.BeginDisabled(true);
                }
                catch (Exception e)
                {
                    ImGuiConsole.Log(e);
                }
            }

            if (dstSrv != null && pfRTV != null && pfSRV != null)
            {
                ImGui.SameLine();

                if (ImGui.Button("Apply") && dstSrv != null)
                {
                    var loader = device.TextureLoader;
                    var image = loader.CaptureTexture(device.Context, dstTex);
                    imagePainter.Load(image);
                    image.Dispose();
                    Discard();
                    Close();
                }

                ImGui.SameLine();

                if (ImGui.Button("Cancel"))
                {
                    Discard();
                    Close();
                }

                if (compute)
                {
                    ImGui.EndDisabled();
                    ImGui.SameLine();
                    if (ImGui.Button("Abort"))
                    {
                        compute = false;
                        side = 0;
                        x = 0;
                        y = 0;
                        step = 0;
                        Discard();
                    }

                    ImGui.SameLine();

                    ImGui.TextColored(new(0, 1, 0, 1), $"Filtering... {(float)step / steps * 100f:n2} ({step} / {steps})");
                }

                for (int i = 0; i < 6; i++)
                {
                    ImGui.Image(pfSRV.Views[i].NativePointer, new(128, 128));
                    if (i != 5)
                        ImGui.SameLine();
                }
            }

            if (compute)
            {
                if (side < 6)
                {
                    if (x < pfSize)
                    {
                        if (y < pfSize)
                        {
                            prefilterFilter.DrawSlice(device.Context, side, x, y, pfTileSize, pfTileSize);
                            y += pfTileSize;
                            step++;
                        }
                        else
                        {
                            x += pfTileSize;
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
                    srcSrv?.Dispose();
                    srcSrv = null;
                }
            }
        }

        public override void Reset()
        {
        }
    }
}