namespace HexaEngine.Materials.Generator.Analyzers
{
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using System.Text;

    public class ComponentMaskNodeAnalyzer : NodeAnalyzer<SwizzleVectorNode>
    {
        public override void Analyze(SwizzleVectorNode node, GenerationContext context, StringBuilder builder)
        {
            var def = context.GetVariableFirstLink(node.In);

            var type = node.Type;
            if (type.IsScalar)
            {
                var output = context.AddVariable(node.Name, node, new(ScalarType.Float), $"{def.Name}.{node.Mask}");
                context.AddRef(def.Name, output);
            }
            if (type.IsVector)
            {
                if (type.VectorType == VectorType.Float2)
                {
                    var output = context.AddVariable(node.Name, node, new(VectorType.Float2), $"{def.Name}.{node.Mask}");
                    context.AddRef(def.Name, output);
                }
                if (type.VectorType == VectorType.Float3)
                {
                    var output = context.AddVariable(node.Name, node, new(VectorType.Float3), $"{def.Name}.{node.Mask}");
                    context.AddRef(def.Name, output);
                }
                if (type.VectorType == VectorType.Float4)
                {
                    var output = context.AddVariable(node.Name, node, new(VectorType.Float4), $"{def.Name}.{node.Mask}");
                    context.AddRef(def.Name, output);
                }
            }
        }
    }
}