namespace HexaEngine.Materials.Generator.Structs
{
    using HexaEngine.Materials.Generator;

    public struct StructDef
    {
        public string Name;
        public SType Type;

        public StructDef(string name, SType type)
        {
            Name = name;
            Type = type;
        }
    }
}