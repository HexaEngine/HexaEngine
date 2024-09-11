namespace HexaEngine.Materials.Generator.Analyzers
{
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using System.Text;

    public class FlipUVNodeAnalyzer : NodeAnalyzer<FlipUVNode>
    {
        public override void Analyze(FlipUVNode node, GenerationContext context, StringBuilder builder)
        {
            var inUV = context.GetVariableFirstLink(node.InUV);
            string outputUV = node.FlipMode switch
            {
                FlipMode.U => $"float2(1.0 - {inUV.Name}.x, {inUV.Name}.y)",
                FlipMode.V => $"float2({inUV.Name}.x, 1.0 - {inUV.Name}.y)",
                FlipMode.Both => $"float2(1.0 - {inUV.Name}.x, 1.0 - {inUV.Name}.y)",
                _ => $"{inUV.Name}",
            };
            var output = context.AddVariable(node.Name, node, new(VectorType.Float2), outputUV);
            context.AddRef(inUV.Name, output);
        }
    }
}