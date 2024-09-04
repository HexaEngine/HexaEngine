namespace HexaEngine.Editor.MaterialEditor.Generator.Analyzers
{
    using HexaEngine.Editor.MaterialEditor.Generator.Enums;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
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