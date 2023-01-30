namespace HexaEngine.Editor.Materials
{
    public struct ConstantBufferDef
    {
        public string Name;
        public VariableType Type;
        public string? StructTypeName;

        public ConstantBufferDef(string name, VariableType type, string? structTypeName)
        {
            Name = name;
            Type = type;
            StructTypeName = structTypeName;
        }
    }
}