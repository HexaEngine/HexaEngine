namespace HexaEngine.Editor.Materials
{
    public struct Variable
    {
        public int Id;
        public string Name;
        public VariableType Type;
        public string? StructTypeName;
        public int Refs;

        public Variable(int id, string name, VariableType type, string? structTypeName)
        {
            Id = id;
            Name = name;
            Type = type;
            StructTypeName = structTypeName;
            Refs = 0;
        }
    }
}