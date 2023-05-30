namespace HexaEngine.Editor.MeshEditor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using ImGuizmoNET;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public struct CBNTBView
    {
        public int Normals;
        public int Tangents;
        public int Bitangents;
        public float Size;
    }

    public unsafe class MeshEditorWindow : EditorWindow
    {
        private readonly OpenFileDialog openDialog = new();
        private IGraphicsDevice device;
        private MeshSelector selector;

        private bool drawGimbal = true;
        private bool drawGrid = true;
        private bool drawObject = true;
        private bool drawWireframe = true;
        private bool drawNormals = false;
        private bool drawTangents = false;
        private bool drawBitangents = false;

        private string CurrentFile;

        private ModelFile model;
        private MeshSource meshSource;

        private Camera camera = new();
        private ConstantBuffer<CBCamera> cameraBuffer;
        private ConstantBuffer<SelectionResult> selectionBuffer;
        private ConstantBuffer<CBNTBView> ntbView;
        private Vector3 center;
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

        public MeshEditorWindow()
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
            ntbView = new(device, CpuAccessFlags.Write);
            selector = new(device, cameraBuffer, sw.Width, sw.Height);
            Application.MainWindow.SwapChain.Resized += Resized;
            SourceViewport = new(sw.Width, sw.Height);
        }

        public override void Dispose()
        {
            cameraBuffer.Dispose();
            selectionBuffer.Dispose();
            ntbView.Dispose();
            selector.Release();
            UnloadModel();
        }

        private void Resized(object? sender, Core.Windows.Events.ResizedEventArgs e)
        {
            selector.Resize(e.NewWidth, e.NewHeight);
            SourceViewport = new(e.NewWidth, e.NewHeight);
        }

        private void HandleInput()
        {
            if (ImGui.IsWindowHovered() && !ImGui.IsAnyItemHovered())
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && meshSource != null && !overGimbal)
                {
                    Vector3 rayDir = Mouse.ScreenToWorld(camera.Transform.Projection, camera.Transform.ViewInv, Viewport);
                    Vector3 rayPos = camera.Transform.GlobalPosition;
                    ray = new(rayPos, Vector3.Normalize(rayDir));

                    var id = meshSource.Data.IntersectRay(ray);

                    if (id > -1)
                    {
                        var pos = meshSource.Data.Positions[id];
                        List<uint> foundVerts = new();
                        SpatialSort sort = new(meshSource.Data.Positions, sizeof(Vector3));
                        var epsilon = ProcessingHelper.ComputePositionEpsilon(meshSource.Data);
                        sort.FindPositions(pos, epsilon, foundVerts);
                        if (ImGui.GetIO().KeyCtrl)
                        {
                            for (int i = 0; i < foundVerts.Count; i++)
                            {
                                selection.Add(foundVerts[i]);
                            }
                        }
                        else
                        {
                            selection.Clear();
                            for (int i = 0; i < foundVerts.Count; i++)
                            {
                                selection.Add(foundVerts[i]);
                            }
                        }
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
                    {
                        delta = Mouse.Delta;
                    }

                    float wheel = 0;
                    if (Keyboard.IsDown(Key.LCtrl))
                    {
                        wheel = Mouse.DeltaWheel.Y;
                    }

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
                        Vector3 pos = SphereHelper.GetCartesianCoordinates(sc) + center;
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

            if (ImGui.BeginMenu("Edit"))
            {
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Center on Zero"))
                {
                    first = true;
                    center = default;
                }
                if (ImGui.MenuItem("Center on Model") && meshSource != null)
                {
                    first = true;
                    center = meshSource.Data.Box.Center;
                }

                ImGui.Separator();

                ImGui.Checkbox("Gimbal", ref drawGimbal);
                ImGui.Checkbox("Grid", ref drawGrid);
                ImGui.Checkbox("Object", ref drawObject);
                ImGui.Checkbox("Wireframe", ref drawWireframe);

                ImGui.Separator();

                ImGui.Checkbox("Normals", ref drawNormals);
                ImGui.Checkbox("Tangents", ref drawTangents);
                ImGui.Checkbox("Bitangents", ref drawBitangents);
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

            if (drawGrid)
            {
                DebugDraw.SetCamera(camera);
                DebugDraw.SetViewport(Viewport);

                DebugDraw.DrawGrid("Grid", Matrix4x4.Identity, 100, new Vector4(1, 1, 1, 0.2f));
            }

            ImGui.PushItemWidth(100);

            ComboEnumHelper<ImGuizmoMode>.Combo("##Mode", ref mode);

            ImGui.PopItemWidth();

            if (ImGui.Button("\xECE9", new(32, 32)))
            {
                operation = ImGuizmoOperation.Translate;
            }
            TooltipHelper.Tooltip("Translate");

            if (ImGui.Button("\xE7AD", new(32, 32)))
            {
                operation = ImGuizmoOperation.Rotate;
            }
            TooltipHelper.Tooltip("Rotate");

            if (ImGui.Button("\xE740", new(32, 32)))
            {
                operation = ImGuizmoOperation.Scale;
            }
            TooltipHelper.Tooltip("Scale");

            if (ImGui.Button("\xE759", new(32, 32)))
            {
                operation = ImGuizmoOperation.Universal;
            }
            TooltipHelper.Tooltip("Translate & Rotate & Scale");

            if (meshSource == null || !meshSource.Overlay.IsValid || !meshSource.Overlay.IsInitialized || !meshSource.Solid.IsValid || !meshSource.Solid.IsInitialized)
            {
                return;
            }

            *cameraBuffer.Local = new(camera);
            cameraBuffer.Update(context);
            ntbView.Local->Normals = drawNormals ? 1 : 0;
            ntbView.Local->Tangents = drawTangents ? 1 : 0;
            ntbView.Local->Bitangents = drawBitangents ? 1 : 0;
            ntbView.Local->Size = 0.05f * meshSource.Data.Box.Extent.Length();
            ntbView.Update(context);

            context.SetRenderTarget(Application.MainWindow.SwapChain.BackbufferRTV, Application.MainWindow.SwapChain.BackbufferDSV);
            context.ClearRenderTargetView(Application.MainWindow.SwapChain.BackbufferRTV, new Vector4(0.2f, 0.2f, 0.2f, 1));
            context.SetViewport(Viewport);
            context.VSSetConstantBuffer(cameraBuffer, 1);
            context.GSSetConstantBuffer(cameraBuffer, 1);
            context.PSSetConstantBuffer(cameraBuffer, 1);
            context.PSSetConstantBuffer(selectionBuffer, 0);

            if (drawObject)
            {
                context.SetGraphicsPipeline(meshSource.Solid);
                meshSource.Draw(context);
            }

            if (drawWireframe)
            {
                context.SetGraphicsPipeline(meshSource.Overlay);
                meshSource.Draw(context);
            }

            context.GSSetConstantBuffer(ntbView, 0);
            context.SetGraphicsPipeline(meshSource.Normals);
            meshSource.Draw(context);

            context.SetGraphicsPipeline(meshSource.Points);
            meshSource.Draw(context);

            context.ClearState();

            overGimbal = false;
            if (drawGimbal)
            {
                var center = selection.ComputeCenter(meshSource.Data);
                var normal = selection.ComputeNormal(meshSource.Data);

                var view = camera.Transform.View;
                var proj = camera.Transform.Projection;

                var translation = Matrix4x4.CreateTranslation(center);

                var transform = translation;
                ImGuizmo.SetDrawlist();
                ImGuizmo.Enable(true);
                ImGuizmo.SetOrthographic(false);
                ImGuizmo.SetRect(x, y, w, h);

                float* snap = null;

                if (ImGui.GetIO().KeyCtrl)
                {
                    float[] snaps = { 1, 1, 1 };
                    snap = (float*)Unsafe.AsPointer(ref snaps[0]);
                }

                if (ImGuizmo.Manipulate(ref view, ref proj, operation, mode, ref transform, null, snap))
                {
                    gimbalGrabbed = true;
                    selection.Transform(ref meshSource.Data, center, transform);
                    meshSource.Update(context, false, true);
                }

                if (!ImGuizmo.IsUsing() && gimbalGrabbed)
                {
                    gimbalGrabbed = false;
                }
                overGimbal = ImGuizmo.IsOver();
            }
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