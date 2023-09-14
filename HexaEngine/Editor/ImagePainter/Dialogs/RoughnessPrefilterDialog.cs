namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Graphics.Filters;
    using ImGuiNET;
    using System;

    public class RoughnessPrefilterDialog : Modal
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
        private IBLRoughnessPrefilter? roughnessPrefilter;
        private IBLRoughnessPrefilterCompute? roughnessPrefilterCompute;
        private float roughness;
        private bool useComputeShader = true;

        private bool compute;

        public override string Name => "IBL Prefilter";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

        public RoughnessPrefilterDialog(ImagePainterWindow imagePainter, IGraphicsDevice device)
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

            roughnessPrefilter?.Dispose();
            roughnessPrefilter = null;
            roughnessPrefilterCompute?.Dispose();
            roughnessPrefilterCompute = null;
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
            ImGui.SliderFloat("Roughness", ref roughness, 0, 1);

            if (ImGui.Button("Filter") && imagePainter.Source != null)
            {
                try
                {
                    Discard();

                    roughnessPrefilter = new(device);
                    roughnessPrefilter.Roughness = roughness;
                    roughnessPrefilterCompute = new(device);
                    roughnessPrefilter.Roughness = roughness;

                    var image = imagePainter.Source.ToScratchImage(device);

                    srcTex = image.CreateTexture2D(device, Usage.Default, BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags.None, ResourceMiscFlag.TextureCube | ResourceMiscFlag.GenerateMips);
                    srcSrv = device.CreateShaderResourceView(srcTex);

                    context.GenerateMips(srcSrv);

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

                    roughnessPrefilter.Targets = dstRTVs;
                    roughnessPrefilter.Source = srcSrv;

                    roughnessPrefilterCompute.Target = dstUav;
                    roughnessPrefilterCompute.Source = srcSrv;

                    compute = true;
                    ImGui.BeginDisabled(true);
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }
            }

            if (dstSrv != null && dstRTVs != null && dstSRVs != null)
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
                    roughnessPrefilterCompute.Roughness = roughness;
                    roughnessPrefilterCompute.Dispatch(context, size, size);
                }
                else
                {
                    roughnessPrefilter.Roughness = roughness;
                    roughnessPrefilter.Draw(context, size, size);
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