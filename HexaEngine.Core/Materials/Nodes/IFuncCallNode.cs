namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials.Pins;

    public interface IFuncCallNode : ITypedNode, IOutNode
    {
        IReadOnlyList<PrimitivePin> Params { get; }

        string Op { get; }
    }

    public interface IOutNode
    {
        PrimitivePin Out { get; }
    }
}