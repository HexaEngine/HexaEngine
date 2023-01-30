namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Editor.Materials.Generator;

    public interface IMathOpNode
    {
        Pin InLeft { get; }
        Pin InRight { get; }
        string Op { get; }
        Pin Out { get; }

        SType Type { get; }
    }
}