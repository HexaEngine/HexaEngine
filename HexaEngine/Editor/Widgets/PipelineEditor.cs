namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Nodes;
    using HexaEngine.Graphics;
    using HexaEngine.Pipelines.Effects;
    using HexaEngine.Rendering;
    using ImGuiNET;
    using imnodesNET;

    public class PipelineEditor : Widget
    {
        private readonly Graph Graph = new();

        public override void Dispose()
        {
        }

        public override void Draw(IGraphicsContext context)
        {
            if (!IsShown) return;
            if (!ImGui.Begin("Graph Editor"))
            {
                ImGui.End();
                return;
            }

            Graph.Draw();

            ImGui.End();
        }

        public override void DrawMenu()
        {
            if (ImGui.MenuItem("Graph"))
            {
                IsShown = true;
            }
        }

        public override void Init(IGraphicsDevice device)
        {
            IsShown = true;
            var nodeEnd = Graph.CreateNode("SwapChain");
            nodeEnd.CreatePin("in Backbuffer", PinKind.Input, PinType.Texture2D, PinShape.QuadFilled);

            var nodeScene = Graph.CreateNode("Scene");
            nodeScene.CreatePin("out Vertices", PinKind.Output, PinType.Vertices, PinShape.TriangleFilled);
            nodeScene.CreatePin("out Lights", PinKind.Output, PinType.Object, PinShape.TriangleFilled);

            var nodePrepass = Graph.CreateNode("GBuffers");
            nodePrepass.CreatePin("in Vertices", PinKind.Input, PinType.Vertices, PinShape.TriangleFilled);
            nodePrepass.CreatePin("out BaseColor + Alpha", PinKind.Output, PinType.Texture2D, PinShape.QuadFilled);
            nodePrepass.CreatePin("out Position + Depth", PinKind.Output, PinType.Texture2D, PinShape.QuadFilled);
            nodePrepass.CreatePin("out Normal + Roughness", PinKind.Output, PinType.Texture2D, PinShape.QuadFilled);
            nodePrepass.CreatePin("out Cleancoat + Metallic", PinKind.Output, PinType.Texture2D, PinShape.QuadFilled);
            nodePrepass.CreatePin("out Emission + Strengh", PinKind.Output, PinType.Texture2D, PinShape.QuadFilled);
            nodePrepass.CreatePin("out Specular + Specular Tint + AO", PinKind.Output, PinType.Texture2D, PinShape.QuadFilled);

            var nodeBRDF = Graph.CreateNode("BRDF");
            nodeBRDF.CreatePin("out Image", PinKind.Output, PinType.Texture2D, PinShape.QuadFilled);
            nodeBRDF.CreatePin("in Lights", PinKind.Input, PinType.Object, PinShape.TriangleFilled);
            nodeBRDF.CreatePin("in BaseColor + Alpha", PinKind.Input, PinType.Texture2D, PinShape.QuadFilled);
            nodeBRDF.CreatePin("in Position + Depth", PinKind.Input, PinType.Texture2D, PinShape.QuadFilled);
            nodeBRDF.CreatePin("in Normal + Roughness", PinKind.Input, PinType.Texture2D, PinShape.QuadFilled);
            nodeBRDF.CreatePin("in Cleancoat + Metallic", PinKind.Input, PinType.Texture2D, PinShape.QuadFilled);
            nodeBRDF.CreatePin("in Emission + Strengh", PinKind.Input, PinType.Texture2D, PinShape.QuadFilled);
            nodeBRDF.CreatePin("in Specular + Specular Tint + AO", PinKind.Input, PinType.Texture2D, PinShape.QuadFilled);
            nodeBRDF.CreatePin("in SSAO", PinKind.Input, PinType.Texture2D, PinShape.QuadFilled);
            nodeBRDF.CreatePin("in Env", PinKind.Input, PinType.TextureCube, PinShape.QuadFilled);
            nodeBRDF.CreatePin("in Env Prefiltered", PinKind.Input, PinType.TextureCube, PinShape.QuadFilled);
            nodeBRDF.CreatePin("in Irradiance", PinKind.Input, PinType.TextureCube, PinShape.QuadFilled);
            nodeBRDF.CreatePin("in BRDF LUT", PinKind.Input, PinType.Texture2D, PinShape.QuadFilled);

            /*
            var node1 = new BRDFLUTNode(device, Graph.GetUniqueId(), "BRDF LUT", Graph);
            Graph.Nodes.Add(node1);

            List<Effect> effects = new()
            {
                new HBAO(device),
                new Tonemap(device),
                new FXAA(device),
            };
            for (int i = 0; i < effects.Count; i++)
            {
                Graph.Nodes.Add(new EffectNode(effects[i], Graph.GetUniqueId(), effects[i].GetType().Name, Graph));
            }*/
        }
    }

    public class EffectNode : Node
    {
        private readonly Effect effect;

        public EffectNode(Effect effect, int id, string name, Graph graph) : base(id, name, graph)
        {
            this.effect = effect;
            CreatePin("out Image", PinKind.Output, PinType.Texture2D, PinShape.QuadFilled);
            foreach (var item in effect.ResourceSlots)
            {
                CreatePin($"{item.Item1}: {item.Item2}", PinKind.Input, item.Item3, PinShape.QuadFilled);
            }
        }
    }

    public class BRDFLUTNode : Node
    {
        private readonly RenderTexture brdflut;
        private readonly IntPtr ptr;

        public BRDFLUTNode(IGraphicsDevice device, int id, string name, Graph graph) : base(id, name, graph)
        {
            brdflut = new(device, TextureDescription.CreateTexture2DWithRTV(512, 512, 1, Format.RG32Float));

            BRDFEffect brdfFilter = new(device);
            brdfFilter.Target = brdflut.RenderTargetView;
            brdfFilter.Draw(device.Context);
            brdfFilter.Target = null;
            brdfFilter.Dispose();
            ptr = ImGuiRenderer.RegisterTexture(brdflut.ResourceView);
            CreatePin("out RGB", PinKind.Output, PinType.Texture2D, PinShape.QuadFilled);
        }

        public override void Draw()
        {
            imnodes.BeginNode(Id);
            imnodes.BeginNodeTitleBar();
            ImGui.Text(Name);
            imnodes.EndNodeTitleBar();

            for (int i = 0; i < Pins.Count; i++)
            {
                Pins[i].Draw();
            }

            ImGui.Image(ptr, new(128, 128));

            imnodes.EndNode();
        }
    }
}