namespace ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Rendering;
    using ImGuiNET;
    using System.Numerics;

    public class ImagePainterWindow : ImGuiWindow
    {
        private IGraphicsDevice device;
        private readonly OpenFileDialog openDialog = new();
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
        private Image image;

        private ITexture2D? source;
        private IShaderResourceView? sourceSRV;
        private IRenderTargetView? sourceRTV;

        private ITexture2D? preview;
        private IRenderTargetView? previewRTV;
        private IShaderResourceView? previewSRV;

        private Quad quad;
        private IGraphicsPipeline copyPipeline;

        private ConstantBuffer<Vector4> colorCB;

        private ISamplerState samplerState;

        protected override string Name => "Image Painter";

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

        public override unsafe void Init(IGraphicsDevice device)
        {
            exporter = new(device);
            this.device = device;
            samplerState = device.CreateSamplerState(SamplerDescription.PointWrap);
            quad = new(device);
            copyPipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/copy/vs.hlsl",
                PixelShader = "effects/copy/ps.hlsl",
            });

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
                    if (ImGui.MenuItem("Open"))
                    {
                        openDialog.Show();
                    }
                    if (ImGui.MenuItem("Save"))
                    {
                    }
                    if (ImGui.MenuItem("Export"))
                    {
                        if (source != null)
                        {
                            exporter.Image = device.TextureLoader.CaptureTexture(device.Context, source);
                            exporter.Show();
                        }
                    }
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
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        private void DrawWindows(IGraphicsContext context)
        {
            exporter.Draw();
            if (openDialog.Draw())
            {
                if (openDialog.Result == OpenFileResult.Ok)
                {
                    UnloadImage();

                    try
                    {
                        var image = device.TextureLoader.LoadFormFile(openDialog.SelectedFile);
                        LoadImage(image);
                        image.Dispose();
                    }
                    catch (Exception)
                    {
                        UnloadImage();
                    }
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

                if (ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
                {
                    var io = ImGui.GetIO();
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

            if (previewSRV != null && previewRTV != null && preview != null && source != null)
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

                context.ClearRenderTargetView(previewRTV, default);
                context.SetRenderTarget(previewRTV, default);
                context.SetViewport(previewRTV.Viewport);
                context.PSSetShaderResource(sourceSRV, 0);
                quad.DrawAuto(context, copyPipeline);
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
                        history?.UndoPush(context);
                        first = true;
                    }
                    isDown = changed;

                    context.PSSetConstantBuffer(colorCB, 0);

                    brushes.Current.Apply(context);

                    if (moved || first)
                    {
                        context.SetRenderTarget(sourceRTV, default);
                        toolbox.Current?.Draw(curPos, context);
                    }
                    else
                    {
                        context.SetRenderTarget(previewRTV, default);
                        toolbox.Current?.DrawPreview(curPos, context);
                    }

                    context.ClearState();
                }

                ImGui.Image(previewSRV.NativePointer, size * zoom);
                focused = ImGui.IsWindowFocused();
            }
        }

        public void Convert(Format format)
        {
            if (source == null)
                return;
            try
            {
                var image = device.TextureLoader.CaptureTexture(device.Context, source);
                var converted = image.Convert(format, TexFilterFlags.Default);
                image.Dispose();
                UnloadImage();
                LoadImage(converted);
                converted.Dispose();
            }
            catch (Exception)
            {
            }
        }

        private void UnloadImage()
        {
            history?.Dispose();

            if (previewSRV != null)
                ImGuiRenderer.Samplers.Remove(previewSRV.NativePointer);

            preview?.Dispose();
            previewSRV?.Dispose();
            previewRTV?.Dispose();
            preview = null;
            previewSRV = null;
            previewRTV = null;

            image?.Dispose();
            source?.Dispose();
            sourceSRV?.Dispose();
            sourceRTV?.Dispose();
            source = null;
            sourceSRV = null;
            sourceRTV = null;
        }

        private void LoadImage(IScratchImage image)
        {
            var format = image.Metadata.Format;

            if (FormatHelper.IsCompressed(format))
            {
                var image1 = image.Decompress(Format.RGBA8UNorm);
                image.Dispose();
                image = image1;
            }

            this.image = new(device, image);
            source = image.CreateTexture2D(device, Usage.Default, BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags.None, ResourceMiscFlag.None);
            var desc = source.Description;
            imageProperties.Metadata = image.Metadata;
            size = new(source.Description.Width, source.Description.Height);

            sourceSRV = device.CreateShaderResourceView(source);
            sourceRTV = device.CreateRenderTargetView(source, new(0, 0, size.X, size.Y));

            desc.CPUAccessFlags = CpuAccessFlags.RW;
            desc.Usage = Usage.Staging;
            desc.BindFlags = BindFlags.None;
            history = new(device, source, 32);

            preview = device.CreateTexture2D(Format.RGBA32Float, source.Description.Width, source.Description.Height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
            previewSRV = device.CreateShaderResourceView(preview);
            previewRTV = device.CreateRenderTargetView(preview, new(0, 0, size.X, size.Y));

            ImGuiRenderer.Samplers.Add(previewSRV.NativePointer, samplerState);
        }

        public override void Dispose()
        {
            UnloadImage();

            quad?.Dispose();
            copyPipeline?.Dispose();

            colorCB.Dispose();

            colorPicker.Dispose();
            toolbox.Dispose();
            imageProperties.Dispose();
            toolProperties.Dispose();
            brushes.Dispose();
        }
    }
}