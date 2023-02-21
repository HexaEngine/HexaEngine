namespace HexaEngine.Editor.Materials.Generator.Structs
{
    using System.Globalization;
    using System.Text;

    public struct UnorderedAccessView
    {
        public int Slot;
        public string Name;
        public SType UavType;
        public SType Type;

        public UnorderedAccessView(string name, SType uavType, SType type)
        {
            Name = name;
            UavType = uavType;
            Type = type;
        }

        public void Build(StringBuilder builder)
        {
            builder.AppendLine($"{UavType.GetTypeName()}<{Type.GetTypeName()}> {Name} : register(u{Slot.ToString(CultureInfo.InvariantCulture)});");
        }
    }
}