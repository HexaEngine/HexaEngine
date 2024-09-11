namespace HexaEngine.Materials.Generator.Structs
{
    using HexaEngine.Materials.Generator;

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
            builder.WriteLine($"{UavType.GetTypeName()}<{Type.GetTypeName()}> {Name};");
        }
    }
}