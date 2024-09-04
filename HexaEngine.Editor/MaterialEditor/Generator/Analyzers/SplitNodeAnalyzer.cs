namespace HexaEngine.Editor.MaterialEditor.Generator.Analyzers
{
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using System.Text;

    public class SplitNodeAnalyzer : NodeAnalyzer<SplitNode>
    {
        public override void Analyze(SplitNode node, GenerationContext context, StringBuilder builder)
        {
            var def = context.GetVariableFirstLink(node.In);
            context.AddVariable(new(context.Mapping[node], def.Name, node.Type, string.Empty, false, true));
        }
    }
}