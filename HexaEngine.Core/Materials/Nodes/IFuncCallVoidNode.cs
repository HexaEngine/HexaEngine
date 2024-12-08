namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials.Pins;

    public interface IFuncCallVoidNode : ITypedNode
    {
        IReadOnlyList<FloatPin> Params { get; }

        string Op { get; }
    }
}