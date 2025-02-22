namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials.Pins;

    public interface IFuncOperatorNode : ITypedNode, IOutNode
    {
        PrimitivePin InLeft { get; }

        PrimitivePin InRight { get; }

        string Op { get; }
    }
}