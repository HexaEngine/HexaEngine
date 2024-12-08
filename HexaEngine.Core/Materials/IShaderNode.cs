namespace HexaEngine.Materials
{
    using HexaEngine.Core.Collections;
    using HexaEngine.Materials.Generator;

    public interface IShaderNode : INode<IShaderNode>
    {
        public string Name { get; }

        public string Description { get; }

        public SType OutputType { get; }
    }

    public class ShaderNode
    {
        public int Id;

        public string Name { get; set; } = null!;
    }
}