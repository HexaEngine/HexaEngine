namespace HexaEngine.Editor.Materials.Generator.Structs
{
    public struct ConstantBufferDef
    {
        public string Name;
        public Type Type;

        public ConstantBufferDef(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}