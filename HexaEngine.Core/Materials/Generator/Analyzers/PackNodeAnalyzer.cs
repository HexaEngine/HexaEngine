namespace HexaEngine.Materials.Generator.Analyzers
{
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using System.Text;

    public class PackNodeAnalyzer : NodeAnalyzer<PackNode>
    {
        public override void Analyze(PackNode node, GenerationContext context, StringBuilder builder)
        {
            var type = node.Type;
            if (type.IsScalar)
            {
                var def = context.GetVariableFirstLink(node.Pins[0]);
                context.AddVariable(node.Name, node, new(ScalarType.Float), $"{def.Name}");
            }
            if (type.IsVector)
            {
                var defX = context.GetVariableFirstLink(node.InPins[0]);
                var defY = context.GetVariableFirstLink(node.InPins[1]);
                var defZ = context.GetVariableFirstLink(node.InPins[2]);
                var defW = context.GetVariableFirstLink(node.InPins[3]);
                if (type.VectorType == VectorType.Float2)
                {
                    context.AddVariable(node.Name, node, new(VectorType.Float2), $"float2({defX.Name},{defY.Name})");
                }
                if (type.VectorType == VectorType.Float3)
                {
                    context.AddVariable(node.Name, node, new(VectorType.Float3), $"float3({defX.Name},{defY.Name},{defZ.Name})");
                }
                if (type.VectorType == VectorType.Float4)
                {
                    context.AddVariable(node.Name, node, new(VectorType.Float4), $"float4({defX.Name},{defY.Name},{defZ.Name},{defW.Name})");
                }
            }
        }
    }
}