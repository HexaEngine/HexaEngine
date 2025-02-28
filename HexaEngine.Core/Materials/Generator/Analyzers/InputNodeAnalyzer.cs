namespace HexaEngine.Materials.Generator.Analyzers
{
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Nodes;
    using System.Text;

    public class InputNodeAnalyzer : NodeAnalyzer<GeometryNode>
    {
        public override void Analyze(GeometryNode node, GenerationContext context, StringBuilder builder)
        {
            context.AddVariable(new(context.Id, context.InputVar.Name, context.InputType, string.Empty, false, true));
        }
    }
}