namespace HexaEngine.Editor.Materials
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Materials.Generator;
    using HexaEngine.Editor.Materials.Nodes;
    using HexaEngine.Editor.Materials.Nodes.Functions;
    using HexaEngine.Editor.Materials.Nodes.Operations;
    using HexaEngine.Editor.Materials.Nodes.Shaders;
    using HexaEngine.Editor.NodeEditor;
    using ImGuiNET;
    using System.Numerics;

    public class MaterialEditor : ImGuiWindow
    {
        private NodeEditor editor = new();
        private InputNode inputNode;
        private OutputNode outputNode;
        private ShaderGenerator generator = new();
        private IGraphicsPipeline pipeline;
        private Sphere sphere;
        private ConstantBuffer<Matrix4x4> world;
        private ConstantBuffer<CBCamera> view;
        private List<TextureFileNode> textureFiles = new();

        public MaterialEditor()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.MenuBar;
        }

        protected override string Name => "Material Editor";

        public override void Init(IGraphicsDevice device)
        {
            inputNode = new(editor.GetUniqueId(), false, false);
            outputNode = new(editor.GetUniqueId(), false, false);
            outputNode.InitTexture(device);
            editor.AddNode(inputNode);
            editor.AddNode(outputNode);

            editor.NodeRemoved += Editor_NodeRemoved;
            editor.Initialize();

            sphere = new(device);
            world = new(device, Matrix4x4.Transpose(Matrix4x4.Identity), CpuAccessFlags.None);
            view = new(device, CpuAccessFlags.Write);
        }

        public override void Dispose()
        {
            pipeline?.Dispose();
            sphere.Dispose();
            world.Dispose();
            view.Dispose();
            textureFiles.Clear();
            editor.Destroy();
            base.Dispose();
        }

        private void Editor_NodeRemoved(object? sender, Node e)
        {
            if (e is TextureFileNode texture)
            {
                textureFiles.Remove(texture);
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Textures"))
                {
                    if (ImGui.MenuItem("Texture File"))
                    {
                        TextureFileNode node = new(context.Device, editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                        textureFiles.Add(node);
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Constants"))
                {
                    if (ImGui.MenuItem("BRDF"))
                    {
                        BRDFNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Splitter"))
                    {
                        SplitNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Packer"))
                    {
                        PackNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Math"))
                {
                    if (ImGui.MenuItem("Add"))
                    {
                        AddNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Subtract"))
                    {
                        SubtractNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Multiply"))
                    {
                        MultiplyNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Divide"))
                    {
                        DivideNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Mix"))
                    {
                        MixNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Cross"))
                    {
                        CrossNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Dot"))
                    {
                        DotNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Min"))
                    {
                        MinNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Max"))
                    {
                        MaxNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Clamp"))
                    {
                        ClampNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Converter"))
                    {
                        ConvertNode node = new(editor.GetUniqueId(), true, false);
                        editor.AddNode(node);
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.MenuItem("Generate"))
                {
                    Directory.CreateDirectory(Paths.CurrentAssetsPath + "generated/" + "shaders/");
                    File.WriteAllText(Paths.CurrentAssetsPath + "generated/" + "shaders/" + "tmp.hlsl", generator.Generate(outputNode));
                    FileSystem.Refresh();
                    pipeline ??= context.Device.CreateGraphicsPipeline(new()
                    {
                        PixelShader = "tmp.hlsl",
                        VertexShader = "generic/mesh.hlsl",
                    },
                        new GraphicsPipelineState()
                        {
                            Blend = BlendDescription.Opaque,
                            BlendFactor = default,
                            DepthStencil = DepthStencilDescription.Default,
                            Rasterizer = RasterizerDescription.CullBack,
                            SampleMask = 0,
                            StencilRef = 0,
                            Topology = PrimitiveTopology.TriangleList
                        });

                    pipeline.Recompile();
                }

                JsonSerializerSettings settings = new()
                {
                    Formatting = Formatting.Indented,
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    ObjectCreationHandling = ObjectCreationHandling.Auto,
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    CheckAdditionalContent = true,
                    MetadataPropertyHandling = MetadataPropertyHandling.Default,
                    NullValueHandling = NullValueHandling.Include,
                };

                if (ImGui.MenuItem("Save"))
                {
                    File.WriteAllText("test.json", JsonConvert.SerializeObject(editor, settings));
                }

                if (ImGui.MenuItem("Load"))
                {
                    var obj = JsonConvert.DeserializeObject<NodeEditor>(File.ReadAllText("test.json"), settings);
                    inputNode = obj.GetNode<InputNode>();
                    outputNode = obj.GetNode<OutputNode>();
                    outputNode.InitTexture(context.Device);
                    obj.Initialize();
                    editor.Destroy();
                    editor = obj;
                }

                ImGui.EndMenuBar();
            }

            if (pipeline != null)
            {
                view[0] = new(outputNode.Camera);
                view.Update(context);

                outputNode.Texture.ClearAndSetTarget(context, default, DepthStencilClearFlags.All);
                for (int i = 0; i < textureFiles.Count; i++)
                {
                    context.PSSetShaderResource(textureFiles[i].Image, i);
                }
                context.VSSetConstantBuffer(world.Buffer, 0);
                context.VSSetConstantBuffer(view.Buffer, 1);

                sphere.DrawAuto(context, pipeline, new(256, 256));
            }

            editor.Draw();
        }
    }
}