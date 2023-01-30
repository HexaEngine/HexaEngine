namespace HexaEngine.Editor.Materials.Generator.Structs
{
    using System.Globalization;
    using System.Text;

    public struct SamplerState
    {
        public int Slot;
        public string Name;
        public SType SamplerType;

        public SamplerState(string name, SType samplerType)
        {
            Name = name;
            SamplerType = samplerType;
        }

        public void Build(StringBuilder builder)
        {
            builder.AppendLine($"{SamplerType.GetTypeName()} {Name} : register(s{Slot.ToString(CultureInfo.InvariantCulture)});");
        }
    }
}