﻿namespace IBLBaker.Widgets
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

    public class IrradianceWidget : Widget, IDisposable
    {
        private FilePicker picker = new() { CurrentFolder = Environment.CurrentDirectory };
        private bool searchPathEnvironment;
        private string pathEnvironment = string.Empty;

        private Mode[] modes = Enum.GetValues<Mode>();
        private string[] modesStrings = Enum.GetValues<Mode>().Select(x => x.ToString()).ToArray();
        private int modeIndex;
        private Mode mode;

        private int irradianceSize = 1024;
        private int irrTileSize = 1024 / 4;

        private ISamplerState? samplerState;
        private RenderTexture? environmentTex;
        private RenderTexture? irradianceTex;
        private RenderTargetViewArray? irrRTV;
        private ShaderResourceViewArray? irrSRV;
        private IntPtr[] irrIds = new IntPtr[6];
        private IrradianceFilterEffect? irradianceFilter;

        private bool compute;
        private int side;
        private int x;
        private int y;
        private int step;
        private int steps;

        public override void Init(IGraphicsDevice device)
        {
            samplerState = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
            irradianceFilter = new(device);
            irradianceFilter.Samplers.Add(new(samplerState, ShaderStage.Pixel, 0));
        }

        public override void Dispose()
        {
            DiscardIrradiance();
            samplerState?.Dispose();
            irradianceFilter?.Dispose();
            environmentTex?.Dispose();
        }

        private void DiscardIrradiance()
        {
            if (irrSRV != null)
                for (int i = 0; i < 6; i++)
                {
                    ImGuiRenderer.UnregisterTexture(irrSRV.Views[i]);
                }
            irrRTV?.Dispose();
            irrRTV = null;
            irrSRV?.Dispose();
            irrSRV = null;
            irradianceTex?.Dispose();
            irradianceTex = null;
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
            if (irradianceFilter == null) return;
            ImGuiWindowFlags flags = ImGuiWindowFlags.None;
            if (irradianceTex != null)
                flags |= ImGuiWindowFlags.UnsavedDocument;
            if (searchPathEnvironment)
                flags |= ImGuiWindowFlags.NoInputs;
            if (!ImGui.Begin("Irradiance", flags))
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

            if (ImGui.InputInt("Size", ref irradianceSize))
            {
            }

            if (ImGui.Button("Compute"))
            {
                if (File.Exists(pathEnvironment))
                {
                    try
                    {
                        var device = context.Device;
                        DiscardIrradiance();
                        environmentTex = ImportTexture(device, device.Context);
                        irradianceTex = new(device, TextureDescription.CreateTextureCubeWithRTV(irradianceSize, 1, Format.RGBA32Float));
                        irrRTV = irradianceTex.CreateRTVArray(device);
                        irrSRV = irradianceTex.CreateSRVArray(device);
                        irradianceFilter.Targets = irrRTV;
                        irradianceFilter.Resources.Clear();
                        irradianceFilter.Resources.Add(new(environmentTex?.ResourceView, ShaderStage.Pixel, 0));
                        for (int i = 0; i < 6; i++)
                        {
                            irrIds[i] = ImGuiRenderer.RegisterTexture(irrSRV.Views[i]);
                        }
                        steps = 6 * (irradianceSize / irrTileSize) * (irradianceSize / irrTileSize);

                        ImGuiConsole.Log(ConsoleMessageType.Log, "Computing irradiance ...");
                        compute = true;
                        ImGui.BeginDisabled(true);
                    }
                    catch (Exception e)
                    {
                        ImGuiConsole.Log(e);
                    }
                }
            }

            if (irradianceTex != null && irrRTV != null && irrSRV != null)
            {
                ImGui.SameLine();

                if (ImGui.Button("Export"))
                {
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Exporting irradiance ...");
                    context.Device.SaveTextureCube((ITexture2D)irradianceTex.Resource, "irr_o.dds");
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Exported irradiance ... ./irr_o.dds");
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

                    ImGui.TextColored(new(0, 1, 0, 1), $"Computing... {(float)step / steps * 100f:n2} ({step} / {steps})");
                }

                for (int i = 0; i < 6; i++)
                {
                    ImGui.Image(irrIds[i], new(128, 128));
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
                    if (x < irradianceSize)
                    {
                        if (y < irradianceSize)
                        {
                            irradianceFilter.DrawSlice(context, side, x, y, irrTileSize, irrTileSize);
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
                    environmentTex?.Dispose();
                    environmentTex = null;
                    ImGuiConsole.Log(ConsoleMessageType.Log, "Computing irradiance ... done!");
                }
            }
        }
    }
}