namespace HexaEngine.Materials.Generator.Analyzers
{
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Nodes;
    using System.Text;

    public class SplitNodeAnalyzer : NodeAnalyzer<SplitNode>
    {
        public override void Analyze(SplitNode node, GenerationContext context, StringBuilder builder)
        {
            if (node.IsUnknown()) return;
            var def = context.GetVariableFirstLink(node.In);
            context.AddVariable(new(context.Mapping[node], def.Name, node.Type, string.Empty, false, true));
        }
    }
}