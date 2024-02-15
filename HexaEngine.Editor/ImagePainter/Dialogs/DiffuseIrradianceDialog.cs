namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Graphics.Filters;

    public class DiffuseIrradianceDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;
        private readonly IGraphicsDevice device;

        private uint size = 256;

        private ITexture2D? srcTex;
        private IShaderResourceView? srcSrv;
        private ITexture2D? dstTex;
        private IShaderResourceView? dstSrv;
        private IUnorderedAccessView? dstUav;
        private RenderTargetViewArray? dstRTVs;
        private ShaderResourceViewArray? dstSRVs;
        private IBLDiffuseIrradiance? diffuseIrradiance;
        private IBLDiffuseIrradianceCompute? diffuseIrradianceCompute;

        private bool useComputeShader = true;
        private bool compute;

        public override string Name => "Bake Irradiance";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

        public DiffuseIrradianceDialog(ImagePainterWindow imagePainter, IGraphicsDevice device)
        {
            this.imagePainter = imagePainter;
            this.device = device;
        }

        private void Discard()
        {
            dstRTVs?.Dispose();
            dstRTVs = null;
            dstSRVs?.Dispose();
            dstSRVs = null;

            srcTex?.Dispose();
            srcTex = null;
            srcSrv?.Dispose();
            srcSrv = null;
            dstTex?.Dispose();
            dstTex = null;
            dstSrv?.Dispose();
            dstSrv = null;
            dstUav?.Dispose();
            dstUav = null;

            diffuseIrradiance?.Dispose();
            diffuseIrradiance = null;
            diffuseIrradianceCompute?.Dispose();
            diffuseIrradianceCompute = null;
        }

        protected override unsafe void DrawContent()
        {
            var context = device.Context;
            if (compute)
            {
                ImGui.BeginDisabled(true);
            }

            ImGui.Checkbox("Use compute shader", ref useComputeShader);
            uint tSize = size;
            ImGui.InputScalar("cube map size", ImGuiDataType.U32, &tSize);
            size = tSize;

            if (ImGui.Button("Compute"))
            {
                if (imagePainter.Source != null)
                {
                    try
                    {
                        Discard();

                        diffuseIrradiance = new(device);
                        diffuseIrradianceCompute = new(device);

                        var image = imagePainter.Source.ToScratchImage(device);

                        srcTex = image.CreateTexture2D(device, Usage.Default, BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags.None, ResourceMiscFlag.TextureCube | ResourceMiscFlag.GenerateMips);
                        srcSrv = device.CreateShaderResourceView(srcTex);

                        device.Context.GenerateMips(srcSrv);

                        var desc = srcTex.Description;
                        desc.Width = desc.Height = (int)size;
                        desc.Usage = Usage.Default;
                        desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource | BindFlags.UnorderedAccess;
                        desc.MipLevels = 1;

                        dstTex = device.CreateTexture2D(desc);
                        dstSrv = device.CreateShaderResourceView(dstTex);
                        dstUav = device.CreateUnorderedAccessView(dstTex, new(dstTex, UnorderedAccessViewDimension.Texture2DArray));

                        dstRTVs = new(device, dstTex, 6, new(desc.Width, desc.Height));
                        dstSRVs = new(device, dstTex, 6);

                        diffuseIrradiance.Targets = dstRTVs;
                        diffuseIrradiance.Source = srcSrv;

                        diffuseIrradianceCompute.Target = dstUav;
                        diffuseIrradianceCompute.Source = srcSrv;

                        compute = true;
                        ImGui.BeginDisabled(true);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e);
                    }
                }
            }

            if (dstTex != null && dstRTVs != null && dstSRVs != null)
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
                        Discard();
                        Close();
                        return;
                    }

                    ImGui.SameLine();

                    ImGui.TextColored(new(0, 1, 0, 1), $"Filtering...");
                }

                for (int i = 0; i < 6; i++)
                {
                    ImGui.Image(dstSRVs.Views[i].NativePointer, new(128, 128));
                    if (i != 5)
                    {
                        ImGui.SameLine();
                    }
                }
            }

            if (compute)
            {
                if (useComputeShader)
                {
                    diffuseIrradianceCompute.Dispatch(context, size, size);
                }
                else
                {
                    diffuseIrradiance.Draw(context, size, size);
                }

                compute = false;
                srcTex?.Dispose();
                srcTex = null;
                srcSrv?.Dispose();
                srcSrv = null;
            }
        }

        public override void Reset()
        {
        }
    }
}