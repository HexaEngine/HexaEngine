namespace HexaEngine.Editor.NodeEditor.Nodes
{
    public interface IMathOpNode
    {
        Pin InLeft { get; }
        Pin InRight { get; }
        string Op { get; }
        Pin Out { get; }
    }
}