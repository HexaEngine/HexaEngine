namespace HexaEngine.Editor.MaterialEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Metadata;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.MaterialEditor.Nodes.Functions;
    using HexaEngine.Editor.MaterialEditor.Nodes.Textures;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using System.Numerics;
    using System.Reflection;
    using System.Text;

    [EditorWindowCategory("Tools")]
    public class MaterialEditorWindow : EditorWindow
    {
        private const string MetadataVersionKey = "MatNodes.Version";
        private const string MetadataKey = "MatNodes.Data";

        private string Version = "1.0.0.1";

        private IGraphicsDevice device;

        private NodeEditor? editor;
        private InputNode geometryNode;
        private BRDFShadingModelNode outputNode;

        private (string, Type)[] intrinsicFuncs;
        private (string, Type)[] operatorFuncs;
        private readonly ShaderGenerator generator = new();
        private bool autoGenerate = true;

        private Task updateMaterialTask;

        private readonly SemaphoreSlim semaphore = new(1);

        private readonly List<TextureFileNode> textureFiles = new();

        private AssetRef assetRef;
        private MaterialFile? material;
        private bool unsavedData;
        private bool nameChanged;
        private bool unsavedDataDialogIsOpen;
        private string path;

        public MaterialEditorWindow()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.MenuBar;

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

                stream = File.OpenRead(path);
                MaterialFile materialFile = MaterialFile.Read(stream);
                this.path = path;
                MaterialFile = materialFile;
                assetRef = value;

                try
                {
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
                    return;

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
                        geometryNode = new(editor.GetUniqueId(), false, false);
                        outputNode = new(editor.GetUniqueId(), false, false);
                        editor.AddNode(geometryNode);
                        editor.AddNode(outputNode);
                        editor.Initialize();
                    }
                    else
                    {
                        editor = NodeEditor.Deserialize(json);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to deserialize node material data: {value.Name}", ex.Message);
                    Logger.Error($"Failed to deserialize node material data: {value.Name}");
                    Logger.Log(ex);

                    editor = new();
                    geometryNode = new(editor.GetUniqueId(), false, false);
                    outputNode = new(editor.GetUniqueId(), false, false);
                    editor.AddNode(geometryNode);
                    editor.AddNode(outputNode);
                    editor.Initialize();
                }

                ExtractProperties(value, editor);
                ExtractTextures(value, editor);
                editor.Minimap = true;
                editor.Location = Hexa.NET.ImNodes.ImNodesMiniMapLocation.TopRight;
                editor.NodePinValueChanged += NodePinValueChanged;
                editor.LinkAdded += LinkAdded;
                editor.LinkRemoved += LinkRemoved;
                geometryNode = editor.GetNode<InputNode>();
                outputNode = editor.GetNode<BRDFShadingModelNode>();

                foreach (var tex in editor.GetNodes<TextureFileNode>())
                {
                    tex.Device = device;
                    tex.Reload();
                    textureFiles.Add(tex);
                }
            }
        }

        protected override string Name { get; } = "Material Editor";

        private static void ExtractProperties(MaterialData material, NodeEditor editor)
        {
            for (int i = 0; i < material.Properties.Count; i++)
            {
                var property = material.Properties[i];

                foreach (var pin in PropertyPin.FindPropertyPins(editor, property.Name))
                {
                    if (property.ValueType == MaterialValueType.Float)
                    {
                        var vec = property.AsFloat();
                        pin.ValueX = vec;
                    }
                    if (property.ValueType == MaterialValueType.Float2)
                    {
                        var vec = property.AsFloat2();
                        pin.ValueX = vec.X;
                        pin.ValueY = vec.Y;
                    }
                    if (property.ValueType == MaterialValueType.Float3)
                    {
                        var vec = property.AsFloat3();
                        pin.ValueX = vec.X;
                        pin.ValueY = vec.Y;
                        pin.ValueZ = vec.Z;
                    }
                    if (property.ValueType == MaterialValueType.Float4)
                    {
                        var vec = property.AsFloat4();
                        pin.ValueX = vec.X;
                        pin.ValueY = vec.Y;
                        pin.ValueZ = vec.Z;
                        pin.ValueW = vec.W;
                    }
                }
            }
        }

        private static void InsertProperties(MaterialData material, NodeEditor editor)
        {
            var outputNode = editor.GetNode<BRDFShadingModelNode>();
            for (int i = 0; i < outputNode.Pins.Count; i++)
            {
                var pin = outputNode.Pins[i];

                if (pin.Kind != PinKind.Input || pin is not PropertyPin propertyPin)
                {
                    continue;
                }

                if (!Enum.TryParse<MaterialPropertyType>(propertyPin.PropertyName, true, out var type))
                    continue;

                var idx = material.GetPropertyIndex(type);
                MaterialProperty property;

                if (idx == -1)
                {
                    MaterialValueType valueType = type switch
                    {
                        MaterialPropertyType.Unknown => MaterialValueType.Unknown,
                        MaterialPropertyType.ColorDiffuse => MaterialValueType.Float4,
                        MaterialPropertyType.ColorAmbient => MaterialValueType.Float4,
                        MaterialPropertyType.ColorSpecular => MaterialValueType.Float4,
                        MaterialPropertyType.ColorTransparent => MaterialValueType.Float4,
                        MaterialPropertyType.ColorReflective => MaterialValueType.Float4,
                        MaterialPropertyType.BaseColor => MaterialValueType.Float4,
                        MaterialPropertyType.Opacity => MaterialValueType.Float,
                        MaterialPropertyType.Specular => MaterialValueType.Float,
                        MaterialPropertyType.SpecularTint => MaterialValueType.Float,
                        MaterialPropertyType.Glossiness => MaterialValueType.Float,
                        MaterialPropertyType.AmbientOcclusion => MaterialValueType.Float,
                        MaterialPropertyType.Metallic => MaterialValueType.Float,
                        MaterialPropertyType.Roughness => MaterialValueType.Float,
                        MaterialPropertyType.Cleancoat => MaterialValueType.Float,
                        MaterialPropertyType.CleancoatGloss => MaterialValueType.Float,
                        MaterialPropertyType.Sheen => MaterialValueType.Float,
                        MaterialPropertyType.SheenTint => MaterialValueType.Float,
                        MaterialPropertyType.Anisotropy => MaterialValueType.Float,
                        MaterialPropertyType.Subsurface => MaterialValueType.Float,
                        MaterialPropertyType.SubsurfaceColor => MaterialValueType.Float3,
                        MaterialPropertyType.Transmission => MaterialValueType.Float3,
                        MaterialPropertyType.Emissive => MaterialValueType.Float3,
                        MaterialPropertyType.EmissiveIntensity => MaterialValueType.Float,
                        MaterialPropertyType.VolumeThickness => MaterialValueType.Float,
                        MaterialPropertyType.VolumeAttenuationDistance => MaterialValueType.Float,
                        MaterialPropertyType.VolumeAttenuationColor => MaterialValueType.Float4,
                        MaterialPropertyType.TwoSided => MaterialValueType.Bool,
                        MaterialPropertyType.ShadingMode => MaterialValueType.Int32,
                        MaterialPropertyType.EnableWireframe => MaterialValueType.Bool,
                        MaterialPropertyType.BlendFunc => MaterialValueType.Int32,
                        MaterialPropertyType.Transparency => MaterialValueType.Float,
                        MaterialPropertyType.BumpScaling => MaterialValueType.Float,
                        MaterialPropertyType.Shininess => MaterialValueType.Float,
                        MaterialPropertyType.ShininessStrength => MaterialValueType.Float,
                        MaterialPropertyType.Reflectance => MaterialValueType.Float,
                        MaterialPropertyType.IOR => MaterialValueType.Float,
                        MaterialPropertyType.DisplacementStrength => MaterialValueType.Float,
                        _ => MaterialValueType.Unknown,
                    };

                    if (valueType == MaterialValueType.Unknown)
                    {
                        Logger.Warn("Unknown property type");
                        continue;
                    }

                    property = new(type.ToString(), type, valueType, Mathematics.Endianness.LittleEndian);
                    material.Properties.Add(property);
                }
                else
                {
                    property = material.Properties[idx];
                }

                if (property.ValueType == MaterialValueType.Float)
                {
                    property.SetFloat(propertyPin.ValueX);
                }
                if (property.ValueType == MaterialValueType.Float2)
                {
                    property.SetFloat2(propertyPin.Vector2);
                }
                if (property.ValueType == MaterialValueType.Float3)
                {
                    property.SetFloat3(propertyPin.Vector3);
                }
                if (property.ValueType == MaterialValueType.Float4)
                {
                    property.SetFloat4(propertyPin.Vector4);
                }
            }
        }

        private static void ExtractTextures(MaterialData material, NodeEditor editor)
        {
            for (int i = 0; i < material.Textures.Count; i++)
            {
                var texture = material.Textures[i];

                if (texture.File == Guid.Empty)
                {
                    continue;
                }

                var textureNode = TextureFileNode.FindTextureFileNode(editor, texture.File);
                if (textureNode != null)
                {
                    continue;
                }

                textureNode = new(editor.GetUniqueId(), true, false, null);
                editor.AddNode(textureNode);
                textureNode.Name = texture.Name;
                textureNode.Path = texture.File;
                textureNode.SamplerDescription = texture.GetSamplerDesc();
                var output = textureNode.Pins[0];

                if (texture.Type == MaterialTextureType.Normal)
                {
                    NormalMapNode normalMapNode = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(normalMapNode);
                    editor.CreateLink(normalMapNode.Pins[1], output);
                    output = normalMapNode.Pins[0];
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, "Normal"))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                    continue;
                }

                if (texture.Type == MaterialTextureType.RoughnessMetallic)
                {
                    SplitNode splitNode = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(splitNode);
                    editor.CreateLink(splitNode.Pins[0], output);
                    output = splitNode.Pins[1];
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, "Roughness"))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                    output = splitNode.Pins[2];
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, "Metallic"))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                    continue;
                }

                foreach (string aliasName in texture.GetNameAlias())
                {
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, aliasName))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                }
            }
        }

        private static void InsertTextures(MaterialData material, NodeEditor editor)
        {
            material.Textures.Clear();
            foreach (var textureNode in editor.GetNodes<TextureFileNode>())
            {
                if (textureNode.Path == Guid.Empty)
                {
                    continue;
                }

                var connection = textureNode.OutColor.FindLink<BRDFShadingModelNode>(PinKind.Input);

                if (connection is not PropertyPin propertyPin)
                    continue;

                if (!Enum.TryParse<MaterialTextureType>(propertyPin.PropertyName, true, out var type))
                    continue;

                var desc = textureNode.SamplerDescription;

                Core.IO.Binary.Materials.MaterialTexture texture;
                texture.Type = type;
                texture.File = textureNode.Path;
                texture.Blend = BlendMode.Default;
                texture.Op = TextureOp.Add;
                texture.Mapping = 0;
                texture.UVWSrc = 0;
                texture.U = Core.IO.Binary.Materials.MaterialTexture.Convert(desc.AddressU);
                texture.V = Core.IO.Binary.Materials.MaterialTexture.Convert(desc.AddressV);
                texture.Flags = TextureFlags.None;

                int index = material.GetTextureIndex(type);

                if (index != -1)
                {
                    material.Textures[index] = texture;
                }
                else
                {
                    material.Textures.Add(texture);
                }
            }
        }

        public void Sort()
        {
            if (editor == null)
            {
                return;
            }

            var groups = NodeEditor.TreeTraversal2(outputNode, true);
            Array.Reverse(groups);
            Vector2 padding = new(10);
            Vector2 pos = padding;

            float maxWidth = 0;
            editor.BeginModify();
            for (int i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                for (int j = 0; j < group.Length; j++)
                {
                    var node = group[j];
                    node.Position = pos;
                    var size = node.Size;
                    pos.Y += size.Y + padding.Y;
                    maxWidth = Math.Max(maxWidth, size.X);
                }

                pos.Y = padding.Y;
                pos.X += maxWidth + padding.X;
                maxWidth = 0;
            }
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
            if (material != null && editor != null)
            {
                material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataVersionKey).Value = Version;
                material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataKey).Value = editor.Serialize();
                InsertProperties(material, editor);
                InsertTextures(material, editor);

                var metadata = assetRef.GetSourceMetadata();
                if (metadata != null)
                {
                    material.Save(metadata.GetFullPath(), Encoding.UTF8);
                    assetRef.GetSourceMetadata()?.Update();
                    if (nameChanged)
                    {
                        metadata.Rename(material.Name);
                    }
                }
            }

            unsavedData = false;
            nameChanged = false;
        }

        public void CreateNew()
        {
            MaterialFile material = new("New Material", Guid.NewGuid(), MaterialData.Empty);
            material.Properties.Add(new("Metallic", MaterialPropertyType.Metallic, Mathematics.Endianness.LittleEndian, 0f));
            material.Properties.Add(new("Roughness", MaterialPropertyType.Roughness, Mathematics.Endianness.LittleEndian, 0.4f));
            material.Properties.Add(new("AmbientOcclusion", MaterialPropertyType.AmbientOcclusion, Mathematics.Endianness.LittleEndian, 1f));
            material.Name = "New Material";
            MaterialFile = material;

            var metadata = SourceAssetsDatabase.CreateFile(SourceAssetsDatabase.GetFreeName("New Material.material"));
            path = metadata.GetFullPath();
            material.Save(path, Encoding.UTF8);
            var artifact = ArtifactDatabase.GetArtifactForSource(metadata.Guid);
            assetRef = artifact?.Guid ?? Guid.Empty;
        }

        protected override void InitWindow(IGraphicsDevice device)
        {
            this.device = device;

            generator.OnPreBuildTable += GeneratorOnPreBuildTable;

            if (material != null)
            {
                MaterialFile = material;
            }

            intrinsicFuncs = Assembly.GetExecutingAssembly().GetTypes().AsParallel().Where(x => x.BaseType == typeof(FuncCallNodeBase)).Select(x => (x.Name.Replace("Node", string.Empty), x)).ToArray();
            intrinsicFuncs = Assembly.GetExecutingAssembly().GetTypes().AsParallel().Where(x => x.BaseType == typeof(FuncCallVoidNodeBase)).Select(x => (x.Name.Replace("Node", string.Empty), x)).ToArray().Union(intrinsicFuncs).OrderBy(x => x.Item1).ToArray();
            operatorFuncs = Assembly.GetExecutingAssembly().GetTypes().AsParallel().Where(x => x.BaseType == typeof(FuncOperatorBaseNode)).Select(x => (x.Name.Replace("Node", string.Empty), x)).ToArray().OrderBy(x => x.Item1).ToArray();
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

        private void GeneratorOnPreBuildTable(VariableTable table)
        {
            table.SetBaseSrv(2);
            table.SetBaseSampler(2);
        }

        protected override void DisposeCore()
        {
            if (editor != null && material != null)
            {
                material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataKey).Value = editor.Serialize();
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

            DrawMenuBar(context);

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
                if (ImGui.CheckboxFlags("Transparent", ref flags, (int)MaterialFlags.Transparent))
                {
                    material.Flags = (MaterialFlags)flags;
                    unsavedData = true;
                    UpdateMaterial();
                }
                if (ImGui.CheckboxFlags("Alpha Test", ref flags, (int)MaterialFlags.AlphaTest))
                {
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

            if (ImGui.IsKeyDown(ImGuiKey.LeftCtrl) && ImGui.IsKeyDown(ImGuiKey.S))
            {
                Save();
            }
        }

        private void DrawMenuBar(IGraphicsContext context)
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

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Nodes"))
                {
                    DrawNodesMenu(context);
                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem("Sort"))
                {
                    Sort();
                }
                if (ImGui.MenuItem("Generate"))
                {
                    UpdateMaterial();
                }

                ImGui.Checkbox("Auto", ref autoGenerate);

                ImGui.EndMenuBar();
            }
        }

        private void DrawNodesMenu(IGraphicsContext context)
        {
            if (editor == null)
            {
                return;
            }

            if (ImGui.BeginMenu("Textures"))
            {
                if (ImGui.MenuItem("Texture File"))
                {
                    TextureFileNode node = new(editor.GetUniqueId(), true, false, context.Device);
                    editor.AddNode(node);
                    textureFiles.Add(node);
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Methods"))
            {
                if (ImGui.MenuItem("Normal Map"))
                {
                    NormalMapNode node = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(node);
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Constants"))
            {
                if (ImGui.MenuItem("Converter"))
                {
                    ConvertNode node = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(node);
                }
                if (ImGui.MenuItem("Constant"))
                {
                    ConstantNode node = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(node);
                }
                if (ImGui.MenuItem("Component Mask"))
                {
                    ComponentMaskNode node = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(node);
                }
                if (ImGui.MenuItem("Components to Vector"))
                {
                    PackNode node = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(node);
                }
                if (ImGui.MenuItem("Vector to Components"))
                {
                    SplitNode node = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(node);
                }
                if (ImGui.MenuItem("Cam pos"))
                {
                    CamPosNode node = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(node);
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Operators"))
            {
                for (int i = 0; i < operatorFuncs.Length; i++)
                {
                    var func = operatorFuncs[i];
                    if (ImGui.MenuItem(func.Item1))
                    {
                        Node node = (Node)Activator.CreateInstance(func.Item2, editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Intrinsic Functions"))
            {
                for (int i = 0; i < intrinsicFuncs.Length; i++)
                {
                    var func = intrinsicFuncs[i];
                    if (ImGui.MenuItem(func.Item1))
                    {
                        Node node = (Node)Activator.CreateInstance(func.Item2, editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                }

                ImGui.EndMenu();
            }
        }

        private void UpdateMaterial()
        {
            if (editor == null)
                return;

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
            semaphore.Wait();

            InsertProperties(material, editor);
            InsertTextures(material, editor);

            ResourceManager.Shared.BeginNoGCRegion();
            ResourceManager.Shared.UpdateMaterial<Model>(material);
            ResourceManager.Shared.EndNoGCRegion();

            Directory.CreateDirectory("generated/" + "shaders/");
            /*
            IOSignature inputSig = new("Pixel",
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

            File.WriteAllText("assets/generated/shaders/generated/usercodeMaterial.hlsl", result);
            */
            semaphore.Release();
        }
    }
}