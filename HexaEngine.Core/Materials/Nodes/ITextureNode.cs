namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;

    public interface ITextureNode
    {
        string Name { get; }

        Pin InUV { get; }

        Pin OutColor { get; }

        Pin OutTex { get; }
    }
}