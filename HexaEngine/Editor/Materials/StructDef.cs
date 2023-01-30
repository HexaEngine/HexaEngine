namespace HexaEngine.Editor.Materials
{
    public struct StructDef
    {
        public string Name;
        public VariableType Type;
        public string? StructTypeName;
        public string? Semantic;

        public StructDef(string name, VariableType type, string? structTypeName, string? semantic)
        {
            Name = name;
            Type = type;
            StructTypeName = structTypeName;
            Semantic = semantic;
        }
    }
}