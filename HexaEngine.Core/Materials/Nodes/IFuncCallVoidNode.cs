namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials.Pins;

    public interface IFuncCallVoidNode : ITypedNode
    {
        IReadOnlyList<PrimitivePin> Params { get; }

        string Op { get; }
    }
}