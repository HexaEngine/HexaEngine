namespace HexaEngine.Editor.MaterialEditor.Generator.Analyzers
{
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using System.Text;

    public class ConstantNodeAnalyzer : NodeAnalyzer<ConstantNode>
    {
        public override void Analyze(ConstantNode node, GenerationContext context, StringBuilder builder)
        {
            var def = context.GetVariableFirstLink(node.Out);
            context.AddVariable(node.Name, node, node.Type, def.Name);
        }
    }
}