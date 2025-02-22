namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Pins;
    using System.Collections.Generic;

    public interface IFuncCallDeclarationNode : ITypedNode, IOutNode
    {
        string MethodName { get; }

        IReadOnlyList<PrimitivePin> Params { get; }

        void DefineMethod(GenerationContext context, VariableTable table);
    }
}