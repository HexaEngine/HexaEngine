namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Pins;
    using System.Collections.Generic;

    public interface IFuncCallDeclarationNode : ITypedNode
    {
        string MethodName { get; }

        FloatPin Out { get; }

        IReadOnlyList<FloatPin> Params { get; }

        void DefineMethod(VariableTable table);
    }
}