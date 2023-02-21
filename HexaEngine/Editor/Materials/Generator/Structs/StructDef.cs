namespace HexaEngine.Editor.Materials.Generator.Structs
{
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