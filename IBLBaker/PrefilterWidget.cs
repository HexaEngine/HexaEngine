namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Pipelines.Effects.Filter;
    using ImGuiNET;
    using System;
    using System.Linq;

    public class PrefilterWidget : ImGuiWindow, IDisposable
    {
        private readonly FilePicker picker = new(Environment.CurrentDirectory);
        private bool searchPathEnvironment;
        private string pathEnvironment = string.Empty;

        private readonly Mode[] modes = Enum.GetValues<Mode>();
        private readonly string[] modesStrings = Enum.GetValues<Mode>().Select(x => x.ToString()).ToArray();
        private int modeIndex;
        private Mode mode;

        private int pfSize = 4096;
        private readonly int pfTileSize = 4096 / 4;

        private ISamplerState? samplerState;
        private Texture? environmentTex;
        private Texture? prefilterTex;
        private RenderTargetViewArray? pfRTV;
        private ShaderResourceViewArray? pfSRV;
        private readonly IntPtr[] pfIds = new IntPtr[6];
        private PreFilter? prefilterFilter;

        private bool compute;
        private int side;
        private int x;
        private int y;
        private int step;
        private int steps;

        protected override string Name => "IBL Prefilter";

        public override void Init(IGraphicsDevice device)
        {
            samplerState = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
            prefilterFilter = new();
            prefilterFilter.Initialize(device, 0, 0).Wait();
        }

        public override void Dispose()
        {
            DiscardIrradiance();
            samplerState?.Dispose();
            prefilterFilter?.Dispose();
            environmentTex?.Dispose();
        }

        private void DiscardIrradiance()
        {
            pfRTV?.Dispose();
            pfRTV = null;
            pfSRV?.Dispose();
            pfSRV = null;
            prefilterTex?.Dispose();
            prefilterTex = null;
        }

        public Texture? ImportTexture(IGraphicsDevice device, IGraphicsContext context)
        {
            switch (mode)
            {
                case Mode.Cube:
                    ImGuiConsole.Log(LogSeverity.Log, "Loading environment ...");
                    Texture tex = new(device, new TextureFileDescription(pathEnvironment, TextureDimension.TextureCube));
                    ImGuiConsole.Log(LogSeverity.Log, "Loaded environment ...");
                    return tex;

                case Mode.Panorama:

                    ImGuiConsole.Log(LogSeverity.Log, "Loading environment ...");
                    Texture source = new(device, new TextureFileDescription(pathEnvironment));
                    ImGuiConsole.Log(LogSeverity.Log, "Loaded environment ...");

                    ImGuiConsole.Log(LogSeverity.Log, "Converting environment to cubemap ...");
                    EquiRectangularToCubeFilter filter = new();
                    filter.Initialize(device, 0, 0).Wait();
                    filter.Source = source.ShaderResourceView;
                    Texture cube1 = new(device, TextureDescription.CreateTextureCubeWithRTV(source.Description.Height, 1, Format.RGBA16Float));
                    var cu = cube1.CreateRTVArray(device);
                    filter.Targets = cu;
                    filter.Draw(context);
                    context.ClearState();
                    context.GenerateMips(cube1.ShaderResourceView ?? throw new Exception("Cannot convert texture!"));
                    ImGuiConsole.Log(LogSeverity.Log, "Converted environment to cubemap ...");
                    ImGuiConsole.Log(LogSeverity.Log, "Exporting environment ...");
                    device.SaveTexture2D((ITexture2D)cube1.Resource, "env_o.dds");
                    ImGuiConsole.Log(LogSeverity.Log, "Exported environment ... ./env_o.dds");
                    cu.Dispose();
                    source.Dispose();
                    filter.Dispose();
                    return cube1;

                default:
                    return null;
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            if (prefilterFilter == null)
            {
                EndWindow();
                return;
            }
            Flags = ImGuiWindowFlags.None;
            if (IsDocked)
                Flags |= ImGuiWindowFlags.NoBringToFrontOnFocus;
            if (prefilterTex != null)
                Flags |= ImGuiWindowFlags.UnsavedDocument;
            if (searchPathEnvironment)
                Flags |= ImGuiWindowFlags.NoInputs;

            if (!ImGui.Begin("Pre-Filter", ref IsShown, Flags))
            {
                ImGui.End();
                return;
            }

            IsDocked = ImGui.IsWindowDocked();

            if (compute)
                ImGui.BeginDisabled(true);

            if (ImGui.Button("..."))
            {
                searchPathEnvironment = true;
            }
            ImGui.SameLine();
            if (ImGui.InputText("Environment", ref pathEnvironment, 1000))
            {
            }

            if (ImGui.Combo("Type", ref modeIndex, modesStrings, modesStrings.Length))
            {
                mode = modes[modeIndex];
            }

            if (ImGui.InputInt("Size", ref pfSize))
            {
            }

            if (ImGui.SliderFloat("Roughness", ref prefilterFilter.Roughness, 0, 1))
            {
            }

            if (ImGui.Button("Filter"))
            {
                if (File.Exists(pathEnvironment))
                {
                    try
                    {
                        var device = context.Device;
                        DiscardIrradiance();
                        environmentTex = ImportTexture(device, device.Context);
                        int numLevels = (int)(1 + MathF.Floor(MathF.Log2(pfSize)));
                        prefilterTex = new(device, TextureDescription.CreateTextureCubeWithRTV(pfSize, numLevels, Format.RGBA32Float, resourceOptionFlags: ResourceMiscFlag.TextureCube | ResourceMiscFlag.GenerateMips));
                        pfRTV = prefilterTex.CreateRTVArray(device);
                        pfSRV = prefilterTex.CreateSRVArray(device);
                        prefilterFilter.Targets = pfRTV;
                        prefilterFilter.Source = environmentTex?.ShaderResourceView;
                        for (int i = 0; i < 6; i++)
                        {
                            pfIds[i] = pfSRV.Views[i].NativePointer;
                        }
                        steps = 6 * (pfSize / pfTileSize) * (pfSize / pfTileSize);

                        ImGuiConsole.Log(LogSeverity.Log, "Pre-Filtering environment ...");
                        compute = true;
                        ImGui.BeginDisabled(true);
                    }
                    catch (Exception e)
                    {
                        ImGuiConsole.Log(e);
                    }
                }
            }

            if (prefilterTex != null && pfRTV != null && pfSRV != null)
            {
                ImGui.SameLine();

                if (ImGui.Button("Export") && prefilterTex.ShaderResourceView != null)
                {
                    ImGuiConsole.Log(LogSeverity.Log, "Exporting prefilter map ...");
                    context.GenerateMips(prefilterTex.ShaderResourceView);
                    context.Device.SaveTextureCube((ITexture2D)prefilterTex.Resource, Format.RGBA8UNorm, "prefilter_o.dds");
                    ImGuiConsole.Log(LogSeverity.Log, "Exported prefilter map ... ./prefilter_o.dds");
                }

                ImGui.SameLine();

                if (ImGui.Button("Discard"))
                {
                    DiscardIrradiance();
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
                        environmentTex?.Dispose();
                        environmentTex = null;
                        ImGuiConsole.Log(LogSeverity.Error, "Computing irradiance ... aborted!");
                    }

                    ImGui.SameLine();

                    ImGui.TextColored(new(0, 1, 0, 1), $"Filtering... {(float)step / steps * 100f:n2} ({step} / {steps})");
                }

                for (int i = 0; i < 6; i++)
                {
                    ImGui.Image(pfIds[i], new(128, 128));
                    if (i != 5)
                        ImGui.SameLine();
                }
            }

            ImGui.End();

            if (searchPathEnvironment && picker.Draw())
            {
                pathEnvironment = picker.SelectedFile;
                searchPathEnvironment = false;
            }

            if (compute)
            {
                if (side < 6)
                {
                    if (x < pfSize)
                    {
                        if (y < pfSize)
                        {
                            prefilterFilter.DrawSlice(context, side, x, y, pfTileSize, pfTileSize);
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
                    environmentTex?.Dispose();
                    environmentTex = null;
                    ImGuiConsole.Log(LogSeverity.Log, "Pre-Filtering ... done!");
                }
            }
        }
    }
}