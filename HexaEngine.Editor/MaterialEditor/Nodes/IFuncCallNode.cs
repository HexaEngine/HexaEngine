namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;

    public interface IFuncCallNode : ITypedNode
    {
        IReadOnlyList<FloatPin> Params { get; }

        string Op { get; }

        Pin Out { get; }
    }
}