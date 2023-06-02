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
    using HexaEngine.Editor.MeshEditor.Dialogs;
    using HexaEngine.Editor.ModelEditor;
    using HexaEngine.Mathematics;
    using ImGuiNET;
    using ImGuizmoNET;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Text;

    public struct CBNTBView
    {
        public int Normals;
        public int Tangents;
        public int Bitangents;
        public float Size;
    }

    public enum MeshEditorMode
    {
        World,
        Local,
        Normal,
        View,
    }

    public unsafe class ModelEditorWindow : EditorWindow
    {
        private readonly OpenFileDialog openDialog = new();
        private readonly SaveFileDialog saveDialog = new();
        private ModelImportDialog importDialog;
        private IGraphicsDevice device;

        private ModelNodes nodesWindow;
        private ModelNodeProperties nodePropertiesWindow;

        private bool drawGimbal = true;
        private bool drawGrid = true;
        private bool drawObject = true;
        private bool drawWireframe = true;
        private bool drawNormals = false;
        private bool drawTangents = false;
        private bool drawBitangents = false;

        private string CurrentFile;

        private ModelFile? model;
        private MeshSource? selectedMesh;
        private MeshSource[]? sources;
        private Node[]? nodes;
        private Node[]? bones;
        private Matrix4x4[]? transforms;
        private Matrix4x4[]? boneTransforms;

        private Camera camera = new();
        private ConstantBuffer<Matrix4x4> worldBuffer;
        private ConstantBuffer<CBCamera> cameraBuffer;
        private ConstantBuffer<CBNTBView> ntbView;
        private StructuredBuffer<Matrix4x4> boneBuffer;
        private Vector3 center;
        private Vector3 sc = new(2, 0, 0);
        private const float speed = 5;
        private static bool first = true;

        private VertexSelection selection = new();
        private Vector3 selectionNormal;

        private bool gimbalGrabbed;
        private bool overGimbal;

        private ImGuizmoOperation operation = ImGuizmoOperation.Translate;
        private MeshEditorMode mode = MeshEditorMode.World;

        protected override string Name => "Model Editor";

        public Viewport SourceViewport = new(1920, 1080);
        public Viewport Viewport;
        private Ray ray;
        private bool meshEditMode;

        public ModelEditorWindow()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoBackground;
            nodesWindow = new(this);
            nodePropertiesWindow = new(this, nodesWindow);
        }

        public ModelFile? Current => model;

        public override void Init(IGraphicsDevice device)
        {
            importDialog = new(device);
            var sw = Application.MainWindow.SwapChain;
            this.device = device;
            worldBuffer = new(device, CpuAccessFlags.Write);
            boneBuffer = new(device, CpuAccessFlags.Write);
            cameraBuffer = new(device, CpuAccessFlags.Write);
            ntbView = new(device, CpuAccessFlags.Write);
            Application.MainWindow.SwapChain.Resized += Resized;
            SourceViewport = new(sw.Width, sw.Height);
        }

        public void Save()
        {
            model?.Save(CurrentFile, Encoding.UTF8);
        }

        public void Save(string dir)
        {
            model?.Save(dir + Path.GetFileName(CurrentFile), Encoding.UTF8);
        }

        public void Load(string path)
        {
            model = ModelFile.LoadExternal(path);
            sources = new MeshSource[model.Meshes.Length];
            for (int i = 0; i < model.Meshes.Length; i++)
            {
                sources[i] = new(device, model.Meshes[i]);
            }
            int nodeCount = 0;
            model.Root.CountNodes(ref nodeCount);
            nodes = new Node[nodeCount];
            int index = 0;
            model.Root.FillNodes(nodes, ref index);
            transforms = new Matrix4x4[nodeCount];

            int boneCount = 0;
            model.Root.CountBones(ref boneCount);
            bones = new Node[boneCount];
            index = 0;
            model.Root.FillBones(bones, ref index);
            boneTransforms = new Matrix4x4[boneCount];
            RecalculateTransforms();
            selection.Clear();
        }

        private void Unload()
        {
            model = null;
            UnloadModel();
        }

        private void UnloadModel()
        {
            if (sources == null)
                return;
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i].Dispose();
            }
            sources = null;
            selectedMesh = null;
            transforms = null;
            nodes = null;
        }

        private void Resized(object? sender, Core.Windows.Events.ResizedEventArgs e)
        {
            SourceViewport = new(e.NewWidth, e.NewHeight);
        }

        public void RecalculateTransforms()
        {
            if (nodes == null || transforms == null) return;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                transforms[i] = node.GetGlobalTransform();
            }

            for (int i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];
                var id = GetBoneIdByName(bone.Parent.Name);
                if (id == -1)
                {
                    id = GetNodeIdByName(bone.Parent.Name);
                    boneTransforms[i] = bone.Transform;
                }
                else
                {
                    boneTransforms[i] = bone.Transform * boneTransforms[id];
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

                if (ImGui.MenuItem("Import"))
                {
                    importDialog.Show();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Save"))
                {
                    Save();
                }

                if (ImGui.MenuItem("Save to"))
                {
                    saveDialog.Show();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Close"))
                {
                    Unload();
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
                if (ImGui.MenuItem("Center on Model") && selectedMesh != null)
                {
                    first = true;
                    center = selectedMesh.Data.Box.Center;
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

            if (saveDialog.Draw())
            {
                if (saveDialog.Result == SaveFileResult.Ok)
                {
                    Save(saveDialog.CurrentFolder);
                }
            }

            importDialog.Draw();
            nodesWindow.DrawWindow(context);
            nodePropertiesWindow.DrawWindow(context);
        }

        private int GetNodeIdByName(string name)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Name == name)
                    return i;
            }
            return -1;
        }

        private int GetBoneIdByName(string name)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i].Name == name)
                    return i;
            }
            return -1;
        }

        private int GetBoneRoot(Node node)
        {
            if (node.Parent.Flags != NodeFlags.Bone)
                return Array.IndexOf(nodes, node.Parent);
            else
                return GetBoneRoot(node.Parent);
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
            DebugDraw.SetCamera(camera);
            DebugDraw.SetViewport(Viewport);

            if (drawGrid)
            {
                DebugDraw.DrawGrid("Grid", Matrix4x4.Identity, 100, new Vector4(1, 1, 1, 0.2f));
            }

            ImGui.PushItemWidth(100);

            ComboEnumHelper<MeshEditorMode>.Combo("##Mode", ref mode);

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

            if (sources == null || nodes == null || transforms == null)
            {
                return;
            }

            *cameraBuffer.Local = new(camera);
            cameraBuffer.Update(context);

            context.SetRenderTarget(Application.MainWindow.SwapChain.BackbufferRTV, Application.MainWindow.SwapChain.BackbufferDSV);
            context.ClearRenderTargetView(Application.MainWindow.SwapChain.BackbufferRTV, new Vector4(0.2f, 0.2f, 0.2f, 1));
            context.SetViewport(Viewport);

            context.VSSetConstantBuffer(cameraBuffer, 1);
            context.GSSetConstantBuffer(cameraBuffer, 1);
            context.PSSetConstantBuffer(cameraBuffer, 1);

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var transform = transforms[i];
                Matrix4x4.Decompose(transform, out _, out var rotation, out _);
                var parentId = Array.IndexOf(nodes, node.Parent);
                if (parentId != -1)
                {
                    var origin = Vector3.Transform(Vector3.Zero, transforms[parentId]);
                    var direction = Vector3.Transform(Vector3.Zero, transform);

                    DebugDraw.DrawLine(node.Name, origin, direction, new Vector4(0.0f, 0.8f, 0.0f, 1));
                    DebugDraw.DrawSphere(node.Name + "Sphere", direction, rotation, ntbView.Local->Size, new Vector4(0.0f, 0.8f, 0.0f, 1));
                }

                *worldBuffer.Local = Matrix4x4.Transpose(transform);
                worldBuffer.Update(context);
                context.VSSetConstantBuffer(worldBuffer, 2);
                for (int j = 0; j < node.Meshes.Count; j++)
                {
                    var meshSource = sources[node.Meshes[j]];
                    if (!meshSource.Overlay.IsValid || !meshSource.Overlay.IsInitialized || !meshSource.Solid.IsValid || !meshSource.Solid.IsInitialized)
                        continue;

                    ntbView.Local->Normals = drawNormals ? 1 : 0;
                    ntbView.Local->Tangents = drawTangents ? 1 : 0;
                    ntbView.Local->Bitangents = drawBitangents ? 1 : 0;
                    ntbView.Local->Size = 0.05f * MathF.Min(meshSource.Data.Box.Extent.Length(), 1);
                    ntbView.Update(context);

                    if (drawObject)
                    {
                        boneBuffer.ResetCounter();

                        for (int k = 0; k < meshSource.Data.BoneCount; k++)
                        {
                            var bone = meshSource.Data.Bones[k];
                            int index = GetBoneIdByName(bone.Name);
                            Matrix4x4.Invert(transforms[0], out var inverseScene);
                            boneBuffer.Add(Matrix4x4.Transpose(meshSource.Data.Bones[k].Offset * boneTransforms[index]));
                        }

                        boneBuffer.Update(context);
                        context.VSSetShaderResource(boneBuffer.SRV, 0);
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

                    //context.SetGraphicsPipeline(meshSource.Points);
                    //meshSource.Draw(context);
                }
            }

            context.ClearState();

            if (drawGimbal && nodesWindow.Selected != null)
            {
                var node = nodesWindow.Selected;
                var id = Array.IndexOf(nodes, node);
                var parent = node.Parent;
                var parentId = GetNodeIdByName(parent?.Name ?? string.Empty);
                Matrix4x4 parentGlobal = Matrix4x4.Identity;
                if (parentId != -1)
                {
                    parentGlobal = transforms[parentId];
                }

                var transform = transforms[id];
                var view = camera.Transform.View;
                var proj = camera.Transform.Projection;

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

                ImGuizmoMode guizmoMode = mode == MeshEditorMode.World ? ImGuizmoMode.World : ImGuizmoMode.Local;

                if (ImGuizmo.Manipulate(ref view, ref proj, operation, guizmoMode, ref transform, null, snap))
                {
                    gimbalGrabbed = true;
                    Matrix4x4.Invert(parentGlobal, out var parentGlobalInverse);
                    node.Transform = transform * parentGlobalInverse;
                    RecalculateTransforms();
                }

                if (!ImGuizmo.IsUsing() && gimbalGrabbed)
                {
                    gimbalGrabbed = false;
                }
                overGimbal = ImGuizmo.IsOver();
            }

            if (selectedMesh == null)
            {
                return;
            }

            overGimbal = false;

            if (drawGimbal && meshEditMode)
            {
                Matrix4x4 rotation = Matrix4x4.Identity;
                if (mode == MeshEditorMode.Normal)
                {
                    MathUtil.AnglesFromNormal(selectionNormal, out float yaw, out float pitch, out float roll);
                    rotation = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, 0);
                }

                var view = camera.Transform.View;
                var proj = camera.Transform.Projection;

                var center = selection.ComputeCenter(selectedMesh.Data);
                var translation = Matrix4x4.CreateTranslation(center);

                var transform = rotation * translation;
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

                ImGuizmoMode guizmoMode = mode == MeshEditorMode.World ? ImGuizmoMode.World : ImGuizmoMode.Local;

                if (ImGuizmo.Manipulate(ref view, ref proj, operation, guizmoMode, ref transform, null, snap))
                {
                    gimbalGrabbed = true;
                    selection.Transform(ref selectedMesh.Data, center, transform);
                    selectedMesh.Update(context, false, true);
                }

                if (!ImGuizmo.IsUsing() && gimbalGrabbed)
                {
                    selectionNormal = selection.ComputeNormal(selectedMesh.Data);
                    gimbalGrabbed = false;
                }
                overGimbal = ImGuizmo.IsOver();
            }
        }

        private void HandleInput()
        {
            if (ImGui.IsWindowHovered() && !ImGui.IsAnyItemHovered())
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && selectedMesh != null && !overGimbal)
                {
                    Vector3 rayDir = Mouse.ScreenToWorld(camera.Transform.Projection, camera.Transform.ViewInv, Viewport);
                    Vector3 rayPos = camera.Transform.GlobalPosition;
                    ray = new(rayPos, Vector3.Normalize(rayDir));

                    var id = selectedMesh.Data.IntersectRay(ray);

                    if (id > -1)
                    {
                        var pos = selectedMesh.Data.Positions[id];
                        List<uint> foundVerts = new();
                        SpatialSort sort = new(selectedMesh.Data.Positions, sizeof(Vector3));
                        var epsilon = ProcessingHelper.ComputePositionEpsilon(selectedMesh.Data);
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

                        selectionNormal = selection.ComputeNormal(selectedMesh.Data);
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

        public override void Dispose()
        {
            Unload();
            worldBuffer.Dispose();
            cameraBuffer.Dispose();
            ntbView.Dispose();
        }
    }
}