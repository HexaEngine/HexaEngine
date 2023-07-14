namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.MaterialEditor.Generator;

    public interface ITypedNode
    {
        SType Type { get; }
    }

    public interface ITypedPin
    {
        SType Type { get; }
    }
}