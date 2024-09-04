namespace HexaEngine.Editor.MaterialEditor.Generator.Analyzers
{
    using HexaEngine.Editor.MaterialEditor.Generator.Structs;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using System.Text;

    public class FuncCallNodeAnalyzer : NodeAnalyzer<IFuncCallNode>
    {
        public override void Analyze(IFuncCallNode node, GenerationContext context, StringBuilder builder)
        {
            Definition[] definitions = new Definition[node.Params.Count];
            for (int i = 0; i < definitions.Length; i++)
            {
                definitions[i] = context.GetVariableFirstLink(node, node.Params[i]);
            }

            context.BuildFunctionCall(definitions, node.Type, (Node)node, node.Op, builder);
        }
    }
}