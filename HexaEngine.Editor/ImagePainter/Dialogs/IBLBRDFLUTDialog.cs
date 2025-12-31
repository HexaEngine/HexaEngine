namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter.Filters;

    public class IBLBRDFLUTDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;

        private uint size = 512;
        private bool multiscatter = false;
        private bool cloth = true;

        private Texture2D? dstTex;
        private BRDFLUT? brdfLut;

        private bool useComputeShader = true;
        private bool compute;

        public override string Name => "Generate BRDF LUT for IBL";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

        public IBLBRDFLUTDialog(ImagePainterWindow imagePainter)
        {
            this.imagePainter = imagePainter;
        }

        private void Discard()
        {
            dstTex?.Dispose();
            dstTex = null;

            brdfLut?.Dispose();
            brdfLut = null;
        }

        protected override unsafe void DrawContent()
        {
            var context = Application.GraphicsContext;
            if (compute)
            {
                ImGui.BeginDisabled(true);
            }

            ImGui.Checkbox("Use compute shader", ref useComputeShader);
            uint tSize = size;
            ImGui.InputScalar("lut size", ImGuiDataType.U32, &tSize);
            size = tSize;

            ImGui.Checkbox("Multiscatter", ref multiscatter);
            ImGui.Checkbox("Cloth", ref cloth);

            if (ImGui.Button("Compute"))
            {
                try
                {
                    Discard();

                    brdfLut = new(multiscatter, cloth);

                    Texture2DDescription desc = new(Format.R32G32B32A32Float, (int)size, (int)size, 1, 1, GpuAccessFlags.RW);

                    dstTex = new(desc);

                    brdfLut.Target = dstTex.RTV;

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
                    var image = loader.CaptureTexture(context, dstTex);
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

                ImGui.Image(dstTex.SRV!.ToTexRef(), new(128, 128));
            }

            if (compute)
            {
                brdfLut!.Draw(context, size, size);

                compute = false;
            }
        }

        public override void Reset()
        {
        }
    }
}