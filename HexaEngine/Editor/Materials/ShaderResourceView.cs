namespace HexaEngine.Editor.Materials
{
    using System.Globalization;
    using System.Text;

    public struct ShaderResourceView
    {
        public int Slot;
        public string Name;
        public VariableType Type;

        public ShaderResourceView(string name, VariableType type)
        {
            Name = name;
            Type = type;
        }

        public void Build(StringBuilder builder)
        {
            builder.AppendLine($"{VariableTable.GetTypeName(Type)} {Name} : register(t{Slot.ToString(CultureInfo.InvariantCulture)});");
        }
    }
}