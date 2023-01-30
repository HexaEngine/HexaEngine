namespace HexaEngine.Editor.Materials
{
    using HexaEngine.Core.Graphics;
    using System.Globalization;
    using System.Text;

    public struct UnorderedAccessView
    {
        public int Slot;
        public string Name;
        public VariableType Type;
        public Format Format;

        public void Build(StringBuilder builder)
        {
            builder.AppendLine($"{VariableTable.GetTypeName(Type)} {Name} : register(u{Slot.ToString(CultureInfo.InvariantCulture)});");
        }
    }
}