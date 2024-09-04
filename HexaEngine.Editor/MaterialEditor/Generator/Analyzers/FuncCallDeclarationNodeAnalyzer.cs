namespace HexaEngine.Editor.MaterialEditor.Generator.Analyzers
{
    using HexaEngine.Editor.MaterialEditor.Generator.Structs;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using System.Text;

    public class FuncCallDeclarationNodeAnalyzer : NodeAnalyzer<IFuncCallDeclarationNode>
    {
        public override void Analyze(IFuncCallDeclarationNode node, GenerationContext context, StringBuilder builder)
        {
            node.DefineMethod(context.Table);
            Definition[] definitions = new Definition[node.Params.Count];
            for (int i = 0; i < definitions.Length; i++)
            {
                definitions[i] = context.GetVariableFirstLink(node, node.Params[i]);
            }

            context.BuildFunctionCall(definitions, node.Type, (Node)node, node.MethodName, builder, false);
        }
    }
}