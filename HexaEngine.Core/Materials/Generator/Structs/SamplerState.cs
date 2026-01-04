namespace HexaEngine.Materials.Generator.Structs
{
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Nodes;

    public struct SamplerState
    {
        public ITextureNode SourceNode;
        public uint Slot;
        public string Name;
        public SType SamplerType;

        public SamplerState(ITextureNode sourceNode, string name, SType samplerType)
        {
            SourceNode = sourceNode;
            Name = name;
            SamplerType = samplerType;
        }

        public void Build(CodeWriter builder)
        {
            builder.WriteLine($"{SamplerType.GetTypeName()} {Name};");
        }
    }
}