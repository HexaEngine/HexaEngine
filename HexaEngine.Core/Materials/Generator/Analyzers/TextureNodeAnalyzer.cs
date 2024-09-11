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
            var tex = context.GetVariableFirstLink(node.InUV);
            var srv = context.AddSrv(node, $"Srv{node.Name}", new(TextureType.Texture2D), new(VectorType.Float4));
            var sampler = context.AddSampler(node, $"Sampler{node.Name}", new(SamplerType.SamplerState));
            context.AddVariable(node.Name, (Node)node, new(VectorType.Float4), $"{srv.Name}.Sample({sampler.Name}, {tex.Name})");
        }
    }
}