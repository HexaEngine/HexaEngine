namespace HexaEngine.Editor.Materials
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Materials.Generator;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Nodes;
    using HexaEngine.Mathematics;
    using ImGuiNET;
    using System.Numerics;

    public class MaterialEditor : ImGuiWindow
    {
        private readonly NodeEditor editor = new();
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

        protected override string Name => "Mat Editor";

        public override void Init(IGraphicsDevice device)
        {
            inputNode = new(editor, false, false);
            outputNode = new(device, editor, false, false);
            editor.AddNode(inputNode);
            editor.AddNode(outputNode);

            editor.NodeRemoved += Editor_NodeRemoved;

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
                        TextureFileNode node = new(context.Device, editor, true, false);
                        editor.AddNode(node);
                        textureFiles.Add(node);
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Constants"))
                {
                    if (ImGui.MenuItem("Float"))
                    {
                        FloatConstantNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Vector2"))
                    {
                        Vector2ConstantNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Vector3"))
                    {
                        Vector3ConstantNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Vector4"))
                    {
                        Vector4ConstantNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Color3"))
                    {
                        Color3ConstantNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Color4"))
                    {
                        Color4ConstantNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Math"))
                {
                    if (ImGui.MenuItem("Add"))
                    {
                        AddNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Subtract"))
                    {
                        SubtractNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Multiply"))
                    {
                        MultiplyNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Divide"))
                    {
                        DivideNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Mix"))
                    {
                        MixNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }
                    if (ImGui.MenuItem("Converter"))
                    {
                        ConverterNode node = new(editor, true, false);
                        editor.AddNode(node);
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.MenuItem("Generate"))
                {
                    Directory.CreateDirectory(Paths.CurrentAssetsPath + "generated/" + "shaders/");
                    File.WriteAllText(Paths.CurrentAssetsPath + "generated/" + "shaders/" + "tmp.hlsl", generator.Generate(outputNode));
                    FileSystem.Refresh();
                    if (pipeline == null)
                    {
                        pipeline = context.Device.CreateGraphicsPipeline(new()
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
                    }

                    pipeline.Recompile();
                }
                ImGui.EndMenuBar();
            }

            if (pipeline != null)
            {
                view[0] = new(outputNode.Camera);
                view.Update(context);

                outputNode.Texture.ClearAndSetTarget(context, default);
                for (int i = 0; i < textureFiles.Count; i++)
                {
                    context.PSSetShaderResource(textureFiles[i].Image, i);
                }
                context.VSSetConstantBuffer(world.Buffer, 0);
                context.VSSetConstantBuffer(view.Buffer, 1);
                sphere.DrawAuto(context, pipeline, new(256, 256));
                context.ClearState();
            }

            editor.Draw();
        }
    }
}