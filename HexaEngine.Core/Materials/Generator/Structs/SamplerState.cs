namespace HexaEngine.Materials.Generator.Structs
{
    using HexaEngine.Materials.Generator;

    public struct SamplerState
    {
        public uint Slot;
        public string Name;
        public SType SamplerType;

        public SamplerState(string name, SType samplerType)
        {
            Name = name;
            SamplerType = samplerType;
        }

        public void Build(CodeWriter builder)
        {
            builder.WriteLine($"{SamplerType.GetTypeName()} {Name};");
        }
    }
}