namespace HexaEngine.Materials.Generator.Analyzers
{
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using System.Globalization;
    using System.Text;

    public class ConvertNodeAnalyzer : NodeAnalyzer<ConvertNode>
    {
        public override void Analyze(ConvertNode node, GenerationContext context, StringBuilder builder)
        {
            var inVal = context.GetVariableFirstLink(node.In);
            var output = context.AddVariable(node.Name, node, new(VectorType.Float4), $"float4({inVal.Name},{node.Value.ToString(CultureInfo.InvariantCulture)})");
            context.AddRef(inVal.Name, output);
        }
    }
}