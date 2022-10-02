namespace IBLBaker.Widgets
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Graphics;
    using HexaEngine.Pipelines.Effects;
    using HexaEngine.Rendering;
    using IBLBaker;
    using ImGuiNET;
    using System;
    using System.Linq;

    public class PrefilterWidget : Widget
    {
        private FilePicker picker = new() { CurrentFolder = Environment.CurrentDirectory };
        private bool searchPathEnvironment;
        private string pathEnvironment = string.Empty;

        private Mode[] modes = Enum.GetValues<Mode>();
        private string[] modesStrings = Enum.GetValues<Mode>().Select(x => x.ToString()).ToArray();
        private int modeIndex;
        private Mode mode;

        private int pfSize = 4096;
        private int pfTileSize = 4096 / 4;

        private ISamplerState samplerState;
        private RenderTexture? environmentTex;
        private RenderTexture? prefilterTex;
        private RenderTargetViewArray? pfRTV;
        private ShaderResourceViewArray? pfSRV;
        private IntPtr[] pfIds = new IntPtr[6];
        private PreFilterEffect prefilterFilter;

        private bool compute;
        private int side;
        private int x;
        private int y;
        private int step;
        private int steps;

        public override void Init(IGraphicsDevice device)
        {
            samplerState = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
            prefilterFilter = new(device);
            prefilterFilter.Samplers.Add(new(samplerState, ShaderStage.Pixel, 0));
        }

        public override void Dispose()
        {
            DiscardIrradiance();
            samplerState.Dispose();
            prefilterFilter.Dispose();
            environmentTex?.Dispose();
        }

        private void DiscardIrradiance()
        {
            if (pfSRV != null)
                for (int i = 0; i < 6; i++)
                {
                    ImGuiRenderer.UnregisterTexture(pfSRV.Views[i]);
                }
            pfRTV?.Dispose();
            pfRTV = null;
            pfSRV?.Dispose();
            pfSRV = null;
            prefilterTex?.Dispose();
            prefilterTex = null;
        }

        public RenderTexture? ImportTexture(IGraphicsDevice device, IGraphicsContext context)
        {
            switch (mode)
            {
                case Mode.Cube:
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Loading environment ...");
                    RenderTexture tex = new(device, new TextureFileDescription(pathEnvironment, TextureDimension.TextureCube));
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Loaded environment ...");
                    return tex;

                case Mode.Panorama:

                    ImGuiConsole.Log(ConsoleMessageType.Log, "Loading environment ...");
                    RenderTexture source = new(device, new TextureFileDescription(pathEnvironment));
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Loaded environment ...");

                    ImGuiConsole.Log(ConsoleMessageType.Log, "Converting environment to cubemap ...");
                    EquiRectangularToCubeEffect filter = new(device);
                    filter.Resources.Add(new(source.ResourceView, ShaderStage.Pixel, 0));
                    filter.Samplers.Add(new(samplerState, ShaderStage.Pixel, 0));
                    RenderTexture cube1 = new(device, TextureDescription.CreateTextureCubeWithRTV(source.Description.Height, 1, Format.RGBA32Float));
                    var cu = cube1.CreateRTVArray(device);
                    filter.Targets = cu;
                    filter.Draw(context);
                    context.ClearState();
                    context.GenerateMips(cube1.ResourceView ?? throw new Exception("Cannot convert texture!"));
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Converted environment to cubemap ...");
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Exporting environment ...");
                    device.SaveTexture2D((ITexture2D)cube1.Resource, "env_o.dds");
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Exported environment ... ./env_o.dds");
                    cu.Dispose();
                    source.Dispose();
                    filter.Samplers.Clear();
                    filter.Dispose();
                    return cube1;

                default:
                    return null;
            }
        }

        public override void Draw(IGraphicsContext context)
        {
            ImGuiWindowFlags flags = ImGuiWindowFlags.None;
            if (prefilterTex != null)
                flags |= ImGuiWindowFlags.UnsavedDocument;
            if (searchPathEnvironment)
                flags |= ImGuiWindowFlags.NoInputs;
            if (!ImGui.Begin("Pre-Filter", flags))
            {
                ImGui.End();
                return;
            }

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
                        prefilterFilter.Resources.Clear();
                        prefilterFilter.Resources.Add(new(environmentTex?.ResourceView, ShaderStage.Pixel, 0));
                        for (int i = 0; i < 6; i++)
                        {
                            pfIds[i] = ImGuiRenderer.RegisterTexture(pfSRV.Views[i]);
                        }
                        steps = 6 * (pfSize / pfTileSize) * (pfSize / pfTileSize);

                        ImGuiConsole.Log(ConsoleMessageType.Log, "Pre-Filtering environment ...");
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

                if (ImGui.Button("Export") && prefilterTex.ResourceView != null)
                {
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Exporting prefilter map ...");
                    context.GenerateMips(prefilterTex.ResourceView);
                    context.Device.SaveTextureCube((ITexture2D)prefilterTex.Resource, Format.RGBA8UNorm, "prefilter_o.dds");
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Exported prefilter map ... ./prefilter_o.dds");
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
                        ImGuiConsole.Log(ConsoleMessageType.Error, "Computing irradiance ... aborted!");
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
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Pre-Filtering ... done!");
                }
            }
        }
    }
}