namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials.Generator;

    public interface ITypedNode
    {
        SType Type { get; }
    }

    public interface ITypedPin
    {
        SType Type { get; }
    }

    public interface IPropertyNode : ITypedNode
    {
    }
}