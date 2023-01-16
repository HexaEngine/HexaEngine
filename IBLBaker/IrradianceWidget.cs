﻿namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Pipelines.Effects.Filter;
    using ImGuiNET;

    public class IrradianceWidget : ImGuiWindow, IDisposable
    {
        private FilePicker picker = new(Environment.CurrentDirectory);
        private bool searchPathEnvironment;
        private string pathEnvironment = string.Empty;

        private Mode[] modes = Enum.GetValues<Mode>();
        private string[] modesStrings = Enum.GetValues<Mode>().Select(x => x.ToString()).ToArray();
        private int modeIndex;
        private Mode mode;

        private int irradianceSize = 1024;
        private int irrTileSize = 1024 / 8;

        private ISamplerState? samplerState;
        private Texture? environmentTex;
        private Texture? irradianceTex;
        private RenderTargetViewArray? irrRTV;
        private ShaderResourceViewArray? irrSRV;
        private IntPtr[] irrIds = new IntPtr[6];
        private IrradianceFilter? irradianceFilter;

        private bool compute;
        private int side;
        private int x;
        private int y;
        private int step;
        private int steps;

        protected override string Name => "Bake Irradiance";

        public override void Init(IGraphicsDevice device)
        {
            samplerState = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
            irradianceFilter = new();
            irradianceFilter.Initialize(device, 0, 0).Wait();
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
            irrRTV?.Dispose();
            irrRTV = null;
            irrSRV?.Dispose();
            irrSRV = null;
            irradianceTex?.Dispose();
            irradianceTex = null;
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
            if (irradianceFilter == null) return;
            Flags = ImGuiWindowFlags.None;
            if (irradianceTex != null)
                Flags |= ImGuiWindowFlags.UnsavedDocument;
            if (searchPathEnvironment)
                Flags |= ImGuiWindowFlags.NoInputs;

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
                        irradianceFilter.Source = environmentTex?.ShaderResourceView;
                        for (int i = 0; i < 6; i++)
                        {
                            irrIds[i] = irrSRV.Views[i].NativePointer;
                        }
                        steps = 6 * (irradianceSize / irrTileSize) * (irradianceSize / irrTileSize);

                        ImGuiConsole.Log(LogSeverity.Log, "Computing irradiance ...");
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
                    ImGuiConsole.Log(LogSeverity.Log, "Exporting irradiance ...");
                    context.Device.SaveTextureCube((ITexture2D)irradianceTex.Resource, "irr_o.dds");
                    ImGuiConsole.Log(LogSeverity.Log, "Exported irradiance ... ./irr_o.dds");
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

                    ImGui.TextColored(new(0, 1, 0, 1), $"Computing... {(float)step / steps * 100f:n2} ({step} / {steps})");
                }

                for (int i = 0; i < 6; i++)
                {
                    ImGui.Image(irrIds[i], new(128, 128));
                    if (i != 5)
                        ImGui.SameLine();
                }
            }

            EndWindow();

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
                    ImGuiConsole.Log(LogSeverity.Log, "Computing irradiance ... done!");
                }
            }
        }
    }
}