﻿namespace HexaEngine.Materials.Generator
{
    public struct Include
    {
        public string Name;

        public void Build(CodeWriter builder)
        {
            builder.WriteLine($@"#include ""{Name}""");
        }
    }
}