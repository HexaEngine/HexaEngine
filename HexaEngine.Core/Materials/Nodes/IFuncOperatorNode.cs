namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;

    public interface IFuncOperatorNode : ITypedNode
    {
        FloatPin InLeft { get; }

        FloatPin InRight { get; }

        string Op { get; }

        Pin Out { get; }
    }
}