namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter.Dialogs;
    using ImGuiNET;
    using System.Numerics;

    public class ImagePainterWindow : EditorWindow
    {
        private IGraphicsDevice device;
        private readonly OpenFileDialog openDialog = new();
        private readonly ModalCollection<Modal> modals = new();

        private string? CurrentFile;

        private ImageExporter exporter;
        private Vector2 lastpos;

        private Vector2 last;
        private bool focused;

        private float zoom = 1;

        private readonly ColorPicker colorPicker = new();
        private readonly Toolbox toolbox = new();
        private readonly ImageProperties imageProperties;
        private readonly ToolProperties toolProperties;
        private readonly Brushes brushes = new();

        private bool isDown;

        private Vector2 size;

        private ImageHistory? history;
        private ImageSource? source;
        private ImageSourceOverlay? overlay;

        private IGraphicsPipeline copyPipeline;

        private ConstantBuffer<Vector4> colorCB;

        private ISamplerState samplerState;

        public ImagePainterWindow()
        {
            imageProperties = new(this);
            toolProperties = new(toolbox);
            colorPicker.Show();
            toolbox.Show();
            imageProperties.Show();
            toolProperties.Show();
            brushes.Show();
            IsShown = true;
            Flags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar;
        }

        protected override string Name => "Image Painter";

        public ImageSource? Source => source;

        protected override unsafe void InitWindow(IGraphicsDevice device)
        {
            exporter = new(device);
            this.device = device;
            samplerState = device.CreateSamplerState(SamplerStateDescription.PointWrap);

            copyPipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/copy/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen);

            colorCB = new(device, CpuAccessFlags.Write);

            colorPicker.Init(device);
            toolbox.Init(device);
            imageProperties.Init(device);
            toolProperties.Init(device);
            brushes.Init(device);
        }

        private void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        modals.GetOrCreate<CreateNewDialog>(() => new(this, device)).Show();
                    }
                    if (ImGui.MenuItem("Open"))
                    {
                        openDialog.Show();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Save"))
                    {
                        if (source != null && CurrentFile != null)
                        {
                            var image = source.ToScratchImage(device);
                            image.SaveToFile(CurrentFile, Core.Graphics.Textures.TexFileFormat.Auto, 0);
                        }
                    }
                    if (ImGui.MenuItem("Export"))
                    {
                        if (source != null)
                        {
                            exporter.Image = source.ToScratchImage(device);
                            exporter.Show();
                        }
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Close"))
                    {
                        UnloadImage();
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo"))
                    {
                        history?.Undo(device.Context);
                    }
                    if (ImGui.MenuItem("Redo"))
                    {
                        history?.Redo(device.Context);
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Windows"))
                {
                    if (ImGui.MenuItem("Toolbox"))
                    {
                        toolbox.Show();
                    }
                    if (ImGui.MenuItem("Color-picker"))
                    {
                        colorPicker.Show();
                    }
                    if (ImGui.MenuItem("Brushes"))
                    {
                        toolProperties.Show();
                    }
                    if (ImGui.MenuItem("Properties"))
                    {
                        imageProperties.Show();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Image"))
                {
                    if (ImGui.MenuItem("Convert Format"))
                    {
                        modals.GetOrCreate<ConvertFormatDialog>(() => new(this, device)).Show();
                    }
                    if (ImGui.MenuItem("Overwrite Format"))
                    {
                        modals.GetOrCreate<OverwriteFormatDialog>(() => new(this, device)).Show();
                    }
                    if (ImGui.MenuItem("Resize"))
                    {
                        modals.GetOrCreate<ResizeDialog>(() => new(this, device)).Show();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Generate MipMaps"))
                    {
                        modals.GetOrCreate<GenerateMipMapsDialog>(() => new(this, device)).Show();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Convert to cube"))
                    {
                        modals.GetOrCreate<ConvertCubeDialog>(() => new(this, device)).Show();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("IBL Diffuse Irradiance"))
                    {
                        modals.GetOrCreate<DiffuseIrradianceDialog>(() => new(this, device)).Show();
                    }

                    if (ImGui.MenuItem("IBL Roughness Prefilter"))
                    {
                        modals.GetOrCreate<RoughnessPrefilterDialog>(() => new(this, device)).Show();
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        private void DrawWindows(IGraphicsContext context)
        {
            modals.Draw();
            exporter.Draw();
            if (openDialog.Draw())
            {
                if (openDialog.Result == OpenFileResult.Ok)
                {
                    Open(openDialog.FullPath);
                }
            }

            colorPicker.DrawWindow(context);
            toolbox.DrawWindow(context);
            imageProperties.DrawWindow(context);
            toolProperties.DrawWindow(context);
            brushes.DrawWindow(context);
        }

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            DrawWindows(context);
            DrawMenuBar();

            var current = ImGui.GetMousePos();
            var delta = -1 * (current - last);
            last = current;

            if (ImGui.IsWindowHovered())
            {
                const float zoom_step = 2.0f;

                bool zoom_changed = false;
                float new_zoom = zoom;
                var io = ImGui.GetIO();
                if (io.KeyCtrl)
                {
                    if (io.MouseWheel > 0.0f)
                    {
                        new_zoom = zoom * zoom_step * io.MouseWheel;
                        zoom_changed = true;
                    }
                    else if (io.MouseWheel < 0.0f)
                    {
                        new_zoom = zoom / (zoom_step * -io.MouseWheel);
                        zoom_changed = true;
                    }

                    if (ImGui.IsKeyPressed(ImGuiKey.KeypadAdd))
                    {
                        new_zoom = zoom * zoom_step;
                        zoom_changed = true;
                    }
                    if (ImGui.IsKeyPressed(ImGuiKey.KeypadSubtract))
                    {
                        new_zoom = zoom / zoom_step;
                        zoom_changed = true;
                    }
                }

                if (zoom_changed)
                {
                    var mouse_position_on_window = ImGui.GetMousePos() - ImGui.GetWindowPos();

                    var mouse_position_on_list = (new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY()) + mouse_position_on_window) / (size * zoom);

                    var new_mouse_position_on_list = mouse_position_on_list * (size * new_zoom);
                    var new_scroll = new_mouse_position_on_list - mouse_position_on_window;
                    ImGui.SetScrollX(new_scroll.X);
                    ImGui.SetScrollY(new_scroll.Y);
                    zoom = new_zoom;
                }

                if (ImGui.IsMouseDown(ImGuiMouseButton.Middle))
                {
                    if (delta.X != 0.0f)
                    {
                        ImGui.SetScrollX(ImGui.GetScrollX() + delta.X);
                    }
                    if (delta.Y != 0.0f)
                    {
                        ImGui.SetScrollY(ImGui.GetScrollY() + delta.Y);
                    }
                }
            }

            if (overlay != null && source != null)
            {
                var cursorPos = ImGui.GetCursorPos() + (ImGui.GetContentRegionAvail() - size * zoom) * 0.5f;
                if (cursorPos.X < 0)
                {
                    cursorPos.X = 0;
                }
                if (cursorPos.Y < 0)
                {
                    cursorPos.Y = 0;
                }
                ImGui.SetCursorPos(cursorPos);

                *colorCB.Local = colorPicker.Color;
                colorCB.Update(context);

                context.ClearRenderTargetView(overlay.RTV, default);
                context.SetRenderTarget(overlay.RTV, default);
                context.SetViewport(overlay.Viewport);
                context.PSSetShaderResource(0, source.SRV);
                context.DrawInstanced(4, 1, 0, 0);
                context.ClearState();

                var curPosGlob = ImGui.GetCursorScreenPos();

                if (ImGui.IsMouseHoveringRect(curPosGlob, curPosGlob + size * zoom) && focused && brushes.Current != null)
                {
                    var curPos = ImGui.GetMousePos() / zoom - curPosGlob / zoom;
                    var curPosD = curPos - lastpos;
                    lastpos = curPos;
                    var changed = ImGui.IsMouseDown(ImGuiMouseButton.Left);
                    var first = false;
                    var moved = ImGui.IsMouseDragging(ImGuiMouseButton.Left) && curPosD != Vector2.Zero;

                    if (changed && !isDown)
                    {
                        context.ClearDepthStencilView(overlay.DSV, DepthStencilClearFlags.All, 1, 0);
                        history?.UndoPush(context);
                        first = true;
                    }
                    isDown = changed;

                    context.PSSetConstantBuffer(0, colorCB);

                    brushes.Current.Apply(context);

                    Vector2 ratio = new Vector2(source.Viewport.Width, source.Viewport.Height) / size;

                    if (moved || first)
                    {
                        context.SetRenderTarget(source.RTV, overlay.DSV);
                        context.SetViewport(source.Viewport);
                        toolbox.Current?.Draw(curPos, ratio, context);
                    }
                    else
                    {
                        context.SetRenderTarget(overlay.RTV, default);
                        toolbox.Current?.DrawPreview(curPos, ratio, context);
                    }

                    context.ClearState();
                }

                ImGui.Image(overlay.SRV.NativePointer, size * zoom);
                focused = ImGui.IsWindowFocused();
            }
        }

        public void Open(string path)
        {
            try
            {
                UnloadImage();
                var image = device.TextureLoader.LoadFormFile(path);
                LoadImage(image);
                image.Dispose();
                CurrentFile = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load file: {path}", ex.Message);
                Logger.Error($"Failed to load file: {path}");
                Logger.Log(ex);
                UnloadImage();
            }
        }

        public void Load(IScratchImage srcImage)
        {
            try
            {
                UnloadImage();
                LoadImage(srcImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load srcImage", ex.Message);
                Logger.Error($"Failed to load srcImage");
                Logger.Log(ex);
            }
        }

        private void UnloadImage()
        {
            history?.Dispose();

            overlay?.Dispose();
            overlay = null;

            imageProperties.Image = null;
            source?.Dispose();
            source = null;
        }

        private void LoadImage(IScratchImage image)
        {
            var format = image.Metadata.Format;

            if (FormatHelper.IsCompressed(format))
            {
                var image1 = image.Decompress(Format.R8G8B8A8UNorm);
                image.Dispose();
                image = image1;
            }

            source = new(device, image);
            imageProperties.Image = source;
            size = new(source.Metadata.Width, source.Metadata.Height);

            history = new(device, source, 32);
            overlay = new(device, source, samplerState);
        }

        protected override void DisposeCore()
        {
            UnloadImage();

            copyPipeline?.Dispose();

            samplerState.Dispose();

            colorCB.Dispose();

            colorPicker.Dispose();
            toolbox.Dispose();
            imageProperties.Dispose();
            toolProperties.Dispose();
            brushes.Dispose();
        }
    }
}