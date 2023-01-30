namespace HexaEngine.Editor.Materials.Generator.Structs
{
    public struct Operation
    {
        public int Id;
        public string Name;
        public Type ReturnType;
        public int Refs;

        public Operation(int id, string name, Type returnType)
        {
            Id = id;
            Name = name;
            ReturnType = returnType;
            Refs = 0;
        }
    }
}