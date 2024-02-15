namespace HexaEngine.Editor.MaterialEditor.Generator.Structs
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using System.Globalization;

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
            builder.WriteLine($"{SamplerType.GetTypeName()} {Name} : register(s{Slot.ToString(CultureInfo.InvariantCulture)});");
        }
    }
}