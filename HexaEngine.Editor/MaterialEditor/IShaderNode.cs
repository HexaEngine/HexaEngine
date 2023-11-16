namespace HexaEngine.Editor.MaterialEditor
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using System.Collections.Generic;
    using System.Text;

    public interface IShaderNode
    {
        public string Name { get; }

        public string Description { get; }

        public SType OutputType { get; }

        public void Generate(VariableTable table, IReadOnlyDictionary<IShaderNode, int> mapping, StringBuilder builder);
    }
}