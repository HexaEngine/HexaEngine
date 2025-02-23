namespace HexaEngine.Materials.Generator.Analyzers
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Nodes;
    using System.Text;

    public class FuncOperatorNodeAnalyzer : NodeAnalyzer<IFuncOperatorNode>
    {
        public override void Analyze(IFuncOperatorNode node, GenerationContext context, StringBuilder builder)
        {
            if (node.IsUnknown()) return;
            var left = context.GetVariableFirstLink(node, node.InLeft);
            var right = context.GetVariableFirstLink(node, node.InRight);
            context.BuildOperatorCall(left, right, node.Type, (Node)node, node.Op, builder);
        }
    }
}