﻿namespace HexaEngine.Editor.Materials
{
    using HexaEngine.Editor.Materials.Generator;
    using System.Collections.Generic;
    using System.Text;

    public interface IShaderNode
    {
        public string Name { get; }

        public string Description { get; }

        public Type OutputType { get; }

        public void Generate(VariableTable table, IReadOnlyDictionary<IShaderNode, int> mapping, StringBuilder builder);
    }
}