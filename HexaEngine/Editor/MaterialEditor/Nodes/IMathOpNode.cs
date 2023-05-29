namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;

    public interface ITypedNode
    {
        SType Type { get; }
    }

    public interface IMathOpNode : ITypedNode
    {
        FloatPin InLeft { get; }

        FloatPin InRight { get; }

        string Op { get; }

        Pin Out { get; }
    }

    public interface IMathFuncNode : ITypedNode
    {
        IReadOnlyList<FloatPin> Params { get; }

        string Op { get; }

        Pin Out { get; }
    }
}