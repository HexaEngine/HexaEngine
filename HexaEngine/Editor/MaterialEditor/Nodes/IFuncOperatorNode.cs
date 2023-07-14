namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;

    public interface IFuncOperatorNode : ITypedNode
    {
        FloatPin InLeft { get; }

        FloatPin InRight { get; }

        string Op { get; }

        Pin Out { get; }
    }
}