namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter;
    using System;
    using HexaEngine.Editor.ImagePainter.Filters;

    public class RoughnessPrefilterDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;

        private uint size = 256;

        private Texture2D? srcTex;
        private Texture2D? dstTex;
        private IBLRoughnessPrefilterCompute? roughnessPrefilterCompute;
        private float roughness;

        private bool compute;

        public override string Name => "IBL Prefilter";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

        public RoughnessPrefilterDialog(ImagePainterWindow imagePainter)
        {
            this.imagePainter = imagePainter;
        }

        private void Discard()
        {
            srcTex?.Dispose();
            srcTex = null;
            dstTex?.Dispose();
            dstTex = null;
            roughnessPrefilterCompute?.Dispose();
            roughnessPrefilterCompute = null;
        }

        protected override unsafe void DrawContent()
        {
            var context = Application.GraphicsContext;
            if (compute)
            {
                ImGui.BeginDisabled(true);
            }

            uint tSize = size;
            ImGui.InputScalar("cube map size", ImGuiDataType.U32, &tSize);
            size = tSize;
            ImGui.SliderFloat("Roughness", ref roughness, 0, 1);

            if (ImGui.Button("Filter") && imagePainter.Source != null)
            {
                try
                {
                    Discard();

                    roughnessPrefilterCompute = new();
                    roughnessPrefilterCompute.Roughness = roughness;

                    var image = imagePainter.Source.ToScratchImage();

                    srcTex = new(new(Format.R16G16B16A16Float, 1, 1, 6, 0, GpuAccessFlags.RW, miscFlags: ResourceMiscFlag.GenerateMips | ResourceMiscFlag.TextureCube), image);
                    srcTex.CreateArraySlices();
                    context.GenerateMips(srcTex.SRV!);

                    var desc = srcTex.Description;
                    desc.Width = desc.Height = (int)size;
                    desc.Usage = Usage.Default;
                    desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource | BindFlags.UnorderedAccess;
                    desc.MipLevels = 1;

                    dstTex = new(desc);
                    dstTex.CreateArraySlices();

                    roughnessPrefilterCompute.Target = dstTex.UAV;
                    roughnessPrefilterCompute.Source = srcTex.SRV;

                    compute = true;
                    ImGui.BeginDisabled(true);
                }
                catch (Exception e)
                {
                    ImagePainterWindow.Logger.Log(e);
                }
            }

            if (dstTex != null)
            {
                ImGui.SameLine();

                if (ImGui.Button("Apply"))
                {
                    var loader = Application.GraphicsDevice.TextureLoader;
                    var image = loader.CaptureTexture(context, dstTex!);
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
                    ImGui.Image(dstTex.SRVArraySlices![i].ToTexRef(), new(128, 128));
                    if (i != 5)
                    {
                        ImGui.SameLine();
                    }
                }
            }

            if (compute)
            {
                roughnessPrefilterCompute!.Roughness = roughness;
                roughnessPrefilterCompute.Dispatch(context, size, size);

                compute = false;
                srcTex?.Dispose();
                srcTex = null;
            }
        }

        public override void Reset()
        {
        }
    }
}