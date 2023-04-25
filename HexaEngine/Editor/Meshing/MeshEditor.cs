namespace HexaEngine.Editor.Meshes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.Meshing;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using ImGuizmoNET;
    using System.Numerics;

    public unsafe class MeshEditor : ImGuiWindow
    {
        private readonly OpenFileDialog openDialog = new();
        private IGraphicsDevice device;
        private MeshSelector selector;

        private string CurrentFile;

        private ModelFile model;
        private MeshSource meshSource;

        private Camera camera = new();
        private ConstantBuffer<CBCamera> cameraBuffer;
        private ConstantBuffer<SelectionResult> selectionBuffer;
        private Vector3 sc = new(2, 0, 0);
        private const float speed = 2;
        private static bool first = true;

        private VertexSelection selection = new();

        private bool gimbalGrabbed;
        private bool overGimbal;

        private ImGuizmoOperation operation = ImGuizmoOperation.Translate;
        private ImGuizmoMode mode = ImGuizmoMode.Local;

        protected override string Name => "Mesh Editor";

        public Viewport SourceViewport = new(1920, 1080);
        public Viewport Viewport;
        private Ray ray;

        public MeshEditor()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoBackground;
        }

        public override void Init(IGraphicsDevice device)
        {
            var sw = Application.MainWindow.SwapChain;
            this.device = device;
            cameraBuffer = new(device, CpuAccessFlags.Write);
            selectionBuffer = new(device, CpuAccessFlags.Write);
            selector = new(device, cameraBuffer, sw.Width, sw.Height);
            Application.MainWindow.SwapChain.Resized += Resized;
            SourceViewport = new(sw.Width, sw.Height);
        }

        private void Resized(object? sender, Core.Windows.Events.ResizedEventArgs e)
        {
            selector.Resize(e.NewWidth, e.NewHeight);
            SourceViewport = new(e.NewWidth, e.NewHeight);
        }

        private void HandleInput()
        {
            if (ImGui.IsWindowHovered())
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && meshSource != null && !overGimbal)
                {
                    Vector3 rayDir = Mouse.ScreenToWorld(camera.Transform.Projection, camera.Transform.ViewInv, Viewport);
                    Vector3 rayPos = camera.Transform.GlobalPosition;
                    ray = new(rayPos, Vector3.Normalize(rayDir));

                    var id = meshSource.Data.IntersectRay(ray);

                    if (id > -1)
                    {
                        if (ImGui.GetIO().KeyCtrl)
                            selection.Add((uint)id);
                        else
                            selection.OverrideAdd((uint)id);
                    }
                    else
                    {
                        selection.Clear();
                    }
                }
                if (ImGui.IsMouseDown(ImGuiMouseButton.Middle) || Keyboard.IsDown(Key.LCtrl) || first)
                {
                    Vector2 delta = Vector2.Zero;
                    if (Mouse.IsDown(MouseButton.Middle))
                        delta = Mouse.Delta;

                    float wheel = 0;
                    if (Keyboard.IsDown(Key.LCtrl))
                        wheel = Mouse.DeltaWheel.Y;

                    // Only update the camera's position if the mouse got moved in either direction
                    if (delta.X != 0f || delta.Y != 0f || wheel != 0f || first)
                    {
                        sc.X += sc.X / 2 * -wheel;

                        // Rotate the camera left and right
                        sc.Y += -delta.X * Time.Delta * speed;

                        // Rotate the camera up and down
                        // Prevent the camera from turning upside down (1.5f = approx. Pi / 2)
                        sc.Z = Math.Clamp(sc.Z + delta.Y * Time.Delta * speed, -MathF.PI / 2, MathF.PI / 2);

                        first = false;

                        // Calculate the cartesian coordinates
                        Vector3 pos = SphereHelper.GetCartesianCoordinates(sc);
                        var orientation = Quaternion.CreateFromYawPitchRoll(-sc.Y, sc.Z, 0);
                        camera.Transform.PositionRotation = (pos, orientation);
                        camera.Transform.Recalculate();
                    }
                }
            }
        }

        private void DrawMenuBar()
        {
            if (!ImGui.BeginMenuBar())
            {
                ImGui.EndMenuBar();
                return;
            }

            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open"))
                {
                    openDialog.Show();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("options"))
            {
                if (ImGui.RadioButton("Translate", operation == ImGuizmoOperation.Translate))
                {
                    operation = ImGuizmoOperation.Translate;
                }

                if (ImGui.RadioButton("Rotate", operation == ImGuizmoOperation.Rotate))
                {
                    operation = ImGuizmoOperation.Rotate;
                }

                if (ImGui.RadioButton("Scale", operation == ImGuizmoOperation.Scale))
                {
                    operation = ImGuizmoOperation.Scale;
                }

                if (ImGui.RadioButton("Local", mode == ImGuizmoMode.Local))
                {
                    mode = ImGuizmoMode.Local;
                }

                ImGui.SameLine();
                if (ImGui.RadioButton("World", mode == ImGuizmoMode.World))
                {
                    mode = ImGuizmoMode.World;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }

        private void DrawWindows(IGraphicsContext context)
        {
            if (openDialog.Draw())
            {
                if (openDialog.Result == OpenFileResult.Ok)
                {
                    Load(openDialog.FullPath);
                }
            }
        }

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            HandleInput();
            DrawMenuBar();
            DrawWindows(context);

            if (meshSource == null || !meshSource.Overlay.IsValid || !meshSource.Overlay.IsInitialized || !meshSource.Solid.IsValid || !meshSource.Solid.IsInitialized) return;

            var position = ImGui.GetWindowPos();
            var size = ImGui.GetWindowSize();
            float ratioX = size.X / SourceViewport.Width;
            float ratioY = size.Y / SourceViewport.Height;
            var s = Math.Min(ratioX, ratioY);
            var w = SourceViewport.Width * s;
            var h = SourceViewport.Height * s;
            var x = position.X + (size.X - w) / 2;
            var y = position.Y + (size.Y - h) / 2;
            Viewport = new Viewport(x, y, w, h);

            *cameraBuffer.Local = new(camera);
            cameraBuffer.Update(context);

            context.SetRenderTarget(Application.MainWindow.SwapChain.BackbufferRTV, Application.MainWindow.SwapChain.BackbufferDSV);
            context.SetViewport(Viewport);
            context.VSSetConstantBuffer(cameraBuffer, 1);
            context.GSSetConstantBuffer(cameraBuffer, 1);
            context.PSSetConstantBuffer(cameraBuffer, 1);
            context.PSSetConstantBuffer(selectionBuffer, 0);
            context.SetGraphicsPipeline(meshSource.Solid);
            meshSource.Draw(context);
            context.SetGraphicsPipeline(meshSource.Overlay);
            meshSource.Draw(context);
            context.SetGraphicsPipeline(meshSource.Normals);
            meshSource.Draw(context);
            context.ClearState();

            overGimbal = false;

            var center = selection.ComputeCenter(meshSource.Data);
            var normal = selection.ComputeNormal(meshSource.Data);

            var view = camera.Transform.View;
            var proj = camera.Transform.Projection;

            var translation = Matrix4x4.CreateTranslation(center);

            var transform = translation;

            ImGuizmo.Enable(true);
            ImGuizmo.SetOrthographic(false);
            ImGuizmo.SetRect(x, y, w, h);

            if (ImGuizmo.Manipulate(ref view, ref proj, operation, mode, ref transform))
            {
                gimbalGrabbed = true;
                selection.Transform(meshSource.Data, center, transform);
                meshSource.Update(context, false, true);
            }

            if (!ImGuizmo.IsUsing() && gimbalGrabbed)
            {
                gimbalGrabbed = false;
            }
            overGimbal = ImGuizmo.IsOver();
        }

        public void Load(string path)
        {
            LoadModel(ModelFile.LoadExternal(path));
        }

        private void LoadModel(ModelFile model)
        {
            meshSource = new(device, model.GetMesh(0));
            selection.Clear();
        }

        private void UnloadModel()
        {
            meshSource.Dispose();
        }
    }
}