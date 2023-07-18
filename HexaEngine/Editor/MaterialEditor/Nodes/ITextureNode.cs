namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;

    public interface ITextureNode
    {
        string Name { get; }

        Pin InUV { get; }
    }
}