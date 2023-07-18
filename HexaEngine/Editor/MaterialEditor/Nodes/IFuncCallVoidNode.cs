namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor.Pins;

    public interface IFuncCallVoidNode : ITypedNode
    {
        IReadOnlyList<FloatPin> Params { get; }

        string Op { get; }
    }
}