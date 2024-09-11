namespace HexaEngine.Materials.Generator.Analyzers
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Structs;
    using HexaEngine.Materials.Nodes;
    using System.Text;

    public class FuncCallVoidNodeAnalyzer : NodeAnalyzer<IFuncCallVoidNode>
    {
        public override void Analyze(IFuncCallVoidNode node, GenerationContext context, StringBuilder builder)
        {
            Definition[] definitions = new Definition[node.Params.Count];
            for (int i = 0; i < definitions.Length; i++)
            {
                definitions[i] = context.GetVariableFirstLink(node, node.Params[i]);
            }

            context.BuildFunctionCall(definitions, (Node)node, node.Op, builder);
        }
    }
}