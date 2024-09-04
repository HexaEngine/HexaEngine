namespace HexaEngine.Editor.MaterialEditor.Generator.Analyzers
{
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using System.Text;

    public class FuncOperatorNodeAnalyzer : NodeAnalyzer<IFuncOperatorNode>
    {
        public override void Analyze(IFuncOperatorNode node, GenerationContext context, StringBuilder builder)
        {
            var left = context.GetVariableFirstLink(node, node.InLeft);
            var right = context.GetVariableFirstLink(node, node.InRight);
            context.BuildOperatorCall(left, right, node.Type, (Node)node, node.Op, builder);
        }
    }
}