namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.NodeEditor.Pins;
    using System.Collections.Generic;

    public interface IFuncCallDeclarationNode : ITypedNode
    {
        string MethodName { get; }

        FloatPin Out { get; }

        IReadOnlyList<FloatPin> Params { get; }

        void DefineMethod(VariableTable table);
    }
}