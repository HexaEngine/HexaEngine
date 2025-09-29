namespace HexaEngine.Materials.Generator.Analyzers
{
    using HexaEngine.Core.Materials.Nodes;
    using HexaEngine.Materials.Generator;
    using System.Text;

    public class PropertyNodeAnalyzer : NodeAnalyzer<PropertyNode>
    {
        public override void Analyze(PropertyNode node, GenerationContext context, StringBuilder builder)
        {
            var def = context.GetDynamicVariableFirstLink(node.Out, out var propertyName);
            context.AddDynamicVariable(node.Name, node, node.Type, def.Name, propertyName);
        }
    }
}