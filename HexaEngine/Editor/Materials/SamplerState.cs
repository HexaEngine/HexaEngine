namespace HexaEngine.Editor.Materials
{
    using System.Globalization;
    using System.Text;

    public struct SamplerState
    {
        public int Slot;
        public string Name;

        public SamplerState(string name)
        {
            Name = name;
        }

        public void Build(StringBuilder builder)
        {
            builder.AppendLine($"SamplerState {Name} : register(s{Slot.ToString(CultureInfo.InvariantCulture)});");
        }
    }
}