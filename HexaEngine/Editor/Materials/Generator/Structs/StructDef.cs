namespace HexaEngine.Editor.Materials.Generator.Structs
{
    public struct StructDef
    {
        public string Name;
        public Type Type;

        public StructDef(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}