namespace HexaEngine.Editor.MaterialEditor.Generator.Analyzers
{
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using System.Text;

    public class InputNodeAnalyzer : NodeAnalyzer<InputNode>
    {
        public override void Analyze(InputNode node, GenerationContext context, StringBuilder builder)
        {
            context.AddVariable(new(context.Id, context.InputVar.Name, context.InputType, string.Empty, false, true));
        }
    }
}