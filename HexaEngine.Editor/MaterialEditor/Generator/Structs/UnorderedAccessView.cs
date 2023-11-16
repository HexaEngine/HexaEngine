namespace HexaEngine.Editor.MaterialEditor.Generator.Structs
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using System.Globalization;
    using System.Text;

    public struct UnorderedAccessView
    {
        public uint Slot;
        public string Name;
        public SType UavType;
        public SType Type;

        public UnorderedAccessView(string name, SType uavType, SType type)
        {
            Name = name;
            UavType = uavType;
            Type = type;
        }

        public void Build(CodeWriter builder)
        {
            builder.WriteLine($"{UavType.GetTypeName()}<{Type.GetTypeName()}> {Name} : register(u{Slot.ToString(CultureInfo.InvariantCulture)});");
        }
    }
}