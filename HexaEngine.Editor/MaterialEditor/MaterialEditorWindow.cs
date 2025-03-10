﻿namespace HexaEngine.Editor.MaterialEditor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Metadata;
    using HexaEngine.Core.Logging;
    using HexaEngine.Core.Materials;
    using HexaEngine.Core.Materials.Nodes.Functions;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Nodes.Functions;
    using HexaEngine.Materials.Nodes.Textures;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using System.Numerics;
    using System.Text;

    [EditorWindowCategory("Tools")]
    public class MaterialEditorWindow : EditorWindow
    {
        internal static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(MaterialEditor));

        private const string MetadataVersionKey = "MatNodes.Version";
        private const string MetadataKey = "MatNodes.Data";
        private const string Version = "1.0.0.1";

        private const string MetadataSurfaceVersionKey = "MatSurface.Version";
        private const string MetadataSurfaceKey = "MatSurface.Data";
        private const string SurfaceVersion = "1.0.0.0";

        private IGraphicsDevice device = null!;

        private ImNodesNodeEditor? editor;
        private InputNode inputNode = null!;
        private OutputNode outputNode = null!;

        private readonly ShaderGenerator generator = new();
        private bool autoGenerate = true;

        private Task? updateMaterialTask;

        private readonly SemaphoreSlim semaphore = new(1);

        private readonly List<TextureFileNode> textureFiles = new();

        private AssetRef assetRef;
        private MaterialFile? material;
        private bool unsavedData;
        private bool nameChanged;
        private bool unsavedDataDialogIsOpen;

        private string? path;

        private bool showCode;

        private string searchNodeString = "";

        public MaterialEditorWindow()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.MenuBar;

            NodeEditorRegistry.RegisterNodeSingleton<SwizzleVectorNode, SwizzleVectorNodeRenderer>();
            NodeEditorRegistry.RegisterNodeSingleton<TypedNodeBase, TypedNodeBaseRenderer>();
            NodeEditorRegistry.RegisterNodeSingleton<ParallaxMapNode, ParallaxMapNodeRenderer>();
            NodeEditorRegistry.RegisterNodeInstanced<TextureFileNode, TextureFileNodeRenderer>();
            NodeEditorRegistry.RegisterNodeSingleton<FlipUVNode, FlipUVNodeRenderer>();

            NodeEditorRegistry.RegisterNodeInstanced<CodeNode, CodeNodeRenderer>();

            Application.OnEditorPlayStateTransition += ApplicationOnEditorPlayStateTransition;
            Application.MainWindow.Closing += MainWindowClosing;
        }

        private void MainWindowClosing(object? sender, Core.Windows.Events.CloseEventArgs e)
        {
            if (unsavedData)
            {
                e.Handled = true;
                if (!unsavedDataDialogIsOpen)
                {
                    MessageBox.Show("(Material Editor) Unsaved changes", $"Do you want to save the changes in material {material?.Name}?", this, (messageBox, state) =>
                    {
                        if (state is not MaterialEditorWindow materialEditor)
                        {
                            return;
                        }

                        if (messageBox.Result == MessageBoxResult.Yes)
                        {
                            materialEditor.Save();
                            Application.MainWindow.Close();
                        }

                        if (messageBox.Result == MessageBoxResult.No)
                        {
                            materialEditor.unsavedData = false;
                            Application.MainWindow.Close();
                        }

                        materialEditor.unsavedDataDialogIsOpen = false;
                    }, MessageBoxType.YesNoCancel);
                    unsavedDataDialogIsOpen = true;
                }
            }
        }

        private void ApplicationOnEditorPlayStateTransition(EditorPlayStateTransitionEventArgs args)
        {
            if (unsavedData && args.NewState == EditorPlayState.Play)
            {
                args.Cancel = true;
                if (!unsavedDataDialogIsOpen)
                {
                    MessageBox.Show("(Material Editor) Unsaved changes", $"Do you want to save the changes in material {material?.Name}?", (this, args), (messageBox, state) =>
                    {
                        if (state is not (MaterialEditorWindow materialEditor, EditorPlayStateTransitionEventArgs args))
                        {
                            return;
                        }

                        if (messageBox.Result == MessageBoxResult.Yes)
                        {
                            materialEditor.Save();
                            SceneWindow.TransitionToState(args.NewState);
                        }

                        if (messageBox.Result == MessageBoxResult.No)
                        {
                            materialEditor.unsavedData = false;
                            SceneWindow.TransitionToState(args.NewState);
                        }

                        materialEditor.unsavedDataDialogIsOpen = false;
                    }, MessageBoxType.YesNoCancel);
                    unsavedDataDialogIsOpen = true;
                }
            }
        }

        public AssetRef Material
        {
            get => assetRef;
            set
            {
                var metadata = value.GetMetadata();
                if (metadata == null || metadata.Type != AssetType.Material)
                {
                    MaterialFile = null;
                    return;
                }

                var sourceMetadata = value.GetSourceMetadata();

                if (sourceMetadata == null)
                {
                    MaterialFile = null;
                    return;
                }

                var path = sourceMetadata.GetFullPath();
                FileStream? stream = null;

                try
                {
                    stream = File.OpenRead(path);
                    MaterialFile materialFile = MaterialFile.ReadFrom(stream);
                    this.path = path;
                    MaterialFile = materialFile;
                    assetRef = value;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    Logger.Error($"Failed to load material file, {path}");
                    MessageBox.Show($"Failed to load material file, {path}", ex.Message);
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }

        public MaterialFile? MaterialFile
        {
            get => material;
            set
            {
                if (device == null)
                {
                    return;
                }

                if (editor != null)
                {
                    editor.NodePinValueChanged -= NodePinValueChanged;
                    editor.LinkAdded -= LinkAdded;
                    editor.LinkRemoved -= LinkRemoved;
                    editor.Destroy();
                    editor = null;
                }

                material = value;

                if (value == null)
                {
                    return;
                }

                var version = value.Metadata.GetOrAdd<MetadataStringEntry>(MetadataVersionKey).Value;
                var json = value.Metadata.GetOrAdd<MetadataStringEntry>(MetadataKey).Value;

                try
                {
                    if (string.IsNullOrEmpty(json) || version != Version)
                    {
                        editor = new();
                        inputNode = new GeometryNode(editor.GetUniqueId(), false, false);
                        outputNode = new BRDFShadingModelNode(editor.GetUniqueId(), false, false);
                        editor.AddNode(inputNode);
                        editor.AddNode(outputNode);
                        editor.Initialize();
                    }
                    else
                    {
                        editor = ImNodesNodeEditor.Deserialize(json);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to deserialize node material data: {value.Name}", ex.Message);
                    Logger.Error($"Failed to deserialize node material data: {value.Name}");
                    Logger.Log(ex);

                    editor = new();
                    inputNode = new GeometryNode(editor.GetUniqueId(), false, false);
                    outputNode = new BRDFShadingModelNode(editor.GetUniqueId(), false, false);
                    editor.AddNode(inputNode);
                    editor.AddNode(outputNode);
                    editor.Initialize();
                }

                MaterialNodeConverter.ExtractProperties(value, editor);
                MaterialNodeConverter.ExtractTextures(value, editor);
                editor.DrawContextMenuCallback = NodeEditorContextMenu;
                editor.Minimap = true;
                editor.Location = Hexa.NET.ImNodes.ImNodesMiniMapLocation.TopRight;
                editor.NodePinValueChanged += NodePinValueChanged;
                editor.LinkAdded += LinkAdded;
                editor.LinkRemoved += LinkRemoved;
                inputNode = editor.GetNode<GeometryNode>();
                outputNode = editor.GetNode<BRDFShadingModelNode>();

                foreach (var tex in editor.GetNodes<TextureFileNode>())
                {
                    tex.Reload();
                    textureFiles.Add(tex);
                }
            }
        }

        private void NodeEditorContextMenu(ImNodesNodeEditor editor, Pin? droppedPin, Vector2 mousePos)
        {
            var node = DrawNodesMenu();
            if (node != null)
            {
                node.Position = mousePos;
                node.setPositionFlags = NodeSetPositionFlags.Screen;
            }
            if (droppedPin != null && node is INodeDropConnector connector)
            {
                connector.Connect(droppedPin);
            }
        }

        protected override string Name { get; } = $"{UwU.PenRuler} Material Editor";

        public void Layout()
        {
            if (editor == null)
            {
                return;
            }

            NodeLayout layout = new();
            editor.BeginModify();
            layout.Layout(outputNode);
            editor.EndModify();
        }

        public void Unload()
        {
            nameChanged = false;
            unsavedData = false;
            MaterialFile = null;
        }

        public void Save()
        {
            try
            {
                if (material != null && editor != null)
                {
                    material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataVersionKey).Value = Version;
                    material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataKey).Value = editor.Serialize();
                    MaterialNodeConverter.InsertProperties(material, editor);
                    MaterialNodeConverter.InsertTextures(material, editor);

                    var metadata = assetRef.GetSourceMetadata();
                    if (metadata != null)
                    {
                        metadata.Lock();
                        material.Save(metadata.GetFullPath(), Encoding.UTF8);
                        metadata.ReleaseLock();
                        metadata.Update();

                        if (nameChanged)
                        {
                            metadata.Rename(material.Name);
                        }
                    }
                }

                unsavedData = false;
                nameChanged = false;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Failed to save material file, {path}");
                MessageBox.Show($"Failed to save material file, {path}", ex.Message);
            }
        }

        public void CreateNew()
        {
            try
            {
                string fileName = SourceAssetsDatabase.GetFreeName("New Material.material");
                MaterialFile material = new(Path.GetFileNameWithoutExtension(fileName), Guid.NewGuid(), MaterialData.Empty);
                material.Properties.Add(new("Metallic", MaterialPropertyType.Metallic, Endianness.LittleEndian, 0f));
                material.Properties.Add(new("Roughness", MaterialPropertyType.Roughness, Endianness.LittleEndian, 0.4f));
                material.Properties.Add(new("AmbientOcclusion", MaterialPropertyType.AmbientOcclusion, Endianness.LittleEndian, 1f));
                MaterialFile = material;

                path = Path.Combine(SourceAssetsDatabase.RootAssetsFolder, fileName);

                SourceAssetMetadata metadata = SourceAssetsDatabase.CreateFile(path);
                material.Save(path, Encoding.UTF8);
                metadata.Update(); // make sure that the artefact database knows about the file.

                Artifact artifact = metadata.GetArtifact()!;
                assetRef = artifact.Guid;
                unsavedData = true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Failed to create material file, {path}");
                MessageBox.Show($"Failed to create material file, {path}", ex.Message);
            }
        }

        protected override void InitWindow(IGraphicsDevice device)
        {
            this.device = device;

            if (material != null)
            {
                MaterialFile = material;
            }
        }

        private void LinkRemoved(object? sender, Link e)
        {
            if (autoGenerate)
            {
                UpdateMaterial();
            }
            unsavedData = true;
        }

        private void LinkAdded(object? sender, Link e)
        {
            if (autoGenerate)
            {
                UpdateMaterial();
            }
            unsavedData = true;
        }

        private void NodePinValueChanged(object? sender, Pin e)
        {
            if (autoGenerate)
            {
                UpdateMaterial();
            }
            unsavedData = true;
        }

        protected override void DisposeCore()
        {
            Unload();
            if (editor != null && material != null)
            {
                editor.Destroy();
                editor = null;
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            HandleHotkeys();

            if (unsavedData)
            {
                Flags |= ImGuiWindowFlags.UnsavedDocument;
            }
            else
            {
                Flags &= ~ImGuiWindowFlags.UnsavedDocument;
            }

            DrawMenuBar();

            if (showCode && material != null)
            {
                if (ImGui.Begin("Code View"u8, ref showCode))
                {
                    if (ImGui.Button("Apply"))
                    {
                        ResourceManager.Shared.BeginNoGCRegion();
                        ResourceManager.Shared.UpdateMaterial(material);
                        ResourceManager.Shared.EndNoGCRegion();
                    }
                    var code = material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataSurfaceKey).Value ?? string.Empty;
                    if (ImGui.InputTextMultiline("##Code", ref code, 4096, ImGui.GetContentRegionAvail()))
                    {
                        material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataSurfaceKey).Value = code;
                    }
                }
                ImGui.End();
            }

            ImGui.BeginTable("Table", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("");

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            editor?.Draw();

            ImGui.TableSetColumnIndex(1);

            if (material != null)
            {
                var name = material.Name;
                if (ImGui.InputText("Name", ref name, 256))
                {
                    material.Name = name;
                    var meta = assetRef.GetMetadata();
                    if (meta != null)
                    {
                        meta.Name = name;
                    }
                    unsavedData = true;
                    nameChanged = true;
                }

                var flags = (int)material.Flags;
                if (ImGuiP.CheckboxFlags("Transparent", ref flags, (int)MaterialFlags.Transparent))
                {
                    material.Flags = (MaterialFlags)flags;
                    unsavedData = true;
                    UpdateMaterial();
                }
                if (ImGuiP.CheckboxFlags("Alpha Test", ref flags, (int)MaterialFlags.AlphaTest))
                {
                    material.Flags = (MaterialFlags)flags;
                    unsavedData = true;
                    UpdateMaterial();
                }
                if (ImGuiP.CheckboxFlags("Depth Test", ref flags, (int)MaterialFlags.DepthTest))
                {
                    material.Flags = (MaterialFlags)flags;
                    unsavedData = true;
                    UpdateMaterial();
                }
                if (ImGuiP.CheckboxFlags("Depth Always", ref flags, (int)MaterialFlags.DepthAlways))
                {
                    material.Flags = (MaterialFlags)flags;
                    unsavedData = true;
                    UpdateMaterial();
                }
                var value = material.GetProperty(MaterialPropertyType.TwoSided).AsBool();
                if (ImGui.Checkbox("Two Sided", ref value))
                {
                    material.SetPropertyBool(MaterialPropertyType.TwoSided, value);
                    material.Flags = (MaterialFlags)flags;
                    unsavedData = true;
                    UpdateMaterial();
                }
            }

            ImGui.EndTable();
        }

        private void HandleHotkeys()
        {
            bool focused = ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows);

            if (!focused)
            {
                return;
            }

            if (ImGuiP.IsKeyDown(ImGuiKey.LeftCtrl) && ImGuiP.IsKeyDown(ImGuiKey.S))
            {
                Save();
            }
        }

        private void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        CreateNew();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Save"))
                    {
                        Save();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Close"))
                    {
                        Unload();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Show Code"))
                    {
                        ShowCode();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Nodes"))
                {
                    DrawNodesMenu();
                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem("Layout"))
                {
                    Layout();
                }

                if (ImGui.MenuItem("Generate"))
                {
                    UpdateMaterial();
                }

                ImGui.Checkbox("Auto", ref autoGenerate);

                ImGui.EndMenuBar();
            }
        }

        private void ShowCode()
        {
            showCode = !showCode;
        }

        private Node? DrawNodesMenu()
        {
            if (editor == null)
            {
                return null;
            }
            Node? createdNode = null;
            ImGui.InputTextWithHint("##Search", "Search...", ref searchNodeString, 1024);

            if (!string.IsNullOrEmpty(searchNodeString))
            {
                DisplayNodes(MaterialNodeRegistry.GetAllFactories(), ref createdNode);
                return createdNode;
            }

            if (ImGui.BeginMenu("Textures"))
            {
                if (MaterialNodeRegistry.TryGetFactories(MaterialNodeType.Texture, out var factories))
                {
                    DisplayNodes(factories, ref createdNode);
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Methods"))
            {
                if (MaterialNodeRegistry.TryGetFactories(MaterialNodeType.Method, out var factories))
                {
                    DisplayNodes(factories, ref createdNode);
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Noise"))
            {
                if (MaterialNodeRegistry.TryGetFactories(MaterialNodeType.Noise, out var factories))
                {
                    DisplayNodes(factories, ref createdNode);
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Constants"))
            {
                if (MaterialNodeRegistry.TryGetFactories(MaterialNodeType.Constant, out var factories))
                {
                    DisplayNodes(factories, ref createdNode);
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Operators"))
            {
                if (MaterialNodeRegistry.TryGetFactories(MaterialNodeType.Operator, out var factories))
                {
                    DisplayNodes(factories, ref createdNode);
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Intrinsic Functions"))
            {
                if (MaterialNodeRegistry.TryGetFactories(MaterialNodeType.Intrinsic, out var factories))
                {
                    DisplayNodes(factories, ref createdNode);
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Custom"))
            {
                if (MaterialNodeRegistry.TryGetFactories(MaterialNodeType.Custom, out var factories))
                {
                    DisplayNodes(factories, ref createdNode);
                }
                ImGui.EndMenu();
            }

            return createdNode;
        }

        private void DisplayNodes(IReadOnlyList<MaterialNodeFactory> factories, ref Node? createdNode)
        {
            bool search = !string.IsNullOrEmpty(searchNodeString);
            foreach (var factory in factories)
            {
                if (search && !factory.Name.Contains(searchNodeString, StringComparison.CurrentCultureIgnoreCase)) continue;
                if (ImGui.MenuItem(factory.Name))
                {
                    createdNode = factory.CreateInstance();
                    createdNode.Id = editor!.GetUniqueId();
                    createdNode.Removable = true;
                    createdNode.IsStatic = false;
                    editor.AddNode(createdNode);
                }
            }
        }

        private void UpdateMaterial()
        {
            if (editor == null)
            {
                return;
            }

            if (updateMaterialTask != null && !updateMaterialTask.IsCompleted)
            {
                updateMaterialTask.ContinueWith(t =>
                {
                    UpdateMaterialTaskVoid();
                });
            }
            else
            {
                updateMaterialTask = Task.Run(UpdateMaterialTaskVoid);
            }
        }

        private void UpdateMaterialTaskVoid()
        {
            if (material == null || editor == null)
                return;
            semaphore.Wait();

            MaterialNodeConverter.InsertProperties(material, editor);
            MaterialNodeConverter.InsertTextures(material, editor);

            try
            {
                IOSignature inputSig = new("Pixel",
            new SignatureDef("color", new(VectorType.Float4)),
            new SignatureDef("pos", new(VectorType.Float4)),
            new SignatureDef("uv", new(VectorType.Float3)),
            new SignatureDef("normal", new(VectorType.Float3)),
            new SignatureDef("tangent", new(VectorType.Float3)),
            new SignatureDef("binormal", new(VectorType.Float3)));

                IOSignature outputSig = new("Material",
                    new SignatureDef("baseColor", new(VectorType.Float4)),
                    new SignatureDef("normal", new(VectorType.Float3)),
                    new SignatureDef("roughness", new(ScalarType.Float)),
                    new SignatureDef("metallic", new(ScalarType.Float)),
                    new SignatureDef("reflectance", new(ScalarType.Float)),
                    new SignatureDef("ao", new(ScalarType.Float)),
                    new SignatureDef("emissive", new(VectorType.Float4)));

                string result = generator.Generate(outputNode, editor.Nodes, "setupMaterial", false, false, inputSig, outputSig);

                material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataSurfaceVersionKey).Value = SurfaceVersion;
                material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataSurfaceKey).Value = result;
            }
            catch (Exception ex)
            {
                Logger.LogAndShowError("Failed to generate shader code", ex);
            }

            ResourceManager.Shared.BeginNoGCRegion();
            ResourceManager.Shared.UpdateMaterial(material);
            ResourceManager.Shared.EndNoGCRegion();

            semaphore.Release();
        }
    }
}