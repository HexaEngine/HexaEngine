namespace HexaEngine.Editor.Materials.Generator.Structs
{
    using System.Globalization;
    using System.Text;

    public struct UnorderedAccessView
    {
        public int Slot;
        public string Name;
        public Type UavType;
        public Type Type;

        public UnorderedAccessView(string name, Type uavType, Type type)
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