﻿namespace HexaEngine.Materials.Generator
{
    public struct Macro
    {
        public string Name;
        public string Definition;

        public void Build(CodeWriter builder)
        {
            builder.WriteLine($"#define {Name} {Definition}");
        }
    }
}