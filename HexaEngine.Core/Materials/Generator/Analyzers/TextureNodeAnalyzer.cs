namespace HexaEngine.Materials.Generator.Analyzers
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using System.Text;

    public class TextureNodeAnalyzer : NodeAnalyzer<ITextureNode>
    {
        public override void Analyze(ITextureNode node, GenerationContext context, StringBuilder builder)
        {
            Structs.Definition tex;
            Structs.ShaderResourceView srv;
            Structs.SamplerState sampler;
            if (node.OutTex.Links.Count != 0 || node.OutColor.Links.Count != 0)
            {
                tex = context.GetVariableFirstLink(node.InUV);
                srv = context.AddSrv(node, $"Srv{node.Name}", new(TextureType.Texture2D), new(VectorType.Float4));
                sampler = context.AddSampler(node, $"Sampler{node.Name}", new(SamplerType.SamplerState));
            }
            else
            {
                return;
            }

            if (node.OutColor.Links.Count == 0)
            {
                context.AddVariable(node.Name, (Node)node, new(VectorType.Unknown), string.Empty);
                return;
            }

            context.AddVariable(node.Name, (Node)node, new(VectorType.Float4), $"{srv.Name}.Sample({sampler.Name}, {tex.Name})");
        }
    }
}