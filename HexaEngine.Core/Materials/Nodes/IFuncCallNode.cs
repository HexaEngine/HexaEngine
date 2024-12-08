namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;

    public interface IFuncCallNode : ITypedNode
    {
        IReadOnlyList<FloatPin> Params { get; }

        string Op { get; }

        Pin Out { get; }
    }
}