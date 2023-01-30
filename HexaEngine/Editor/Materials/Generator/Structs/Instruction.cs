namespace HexaEngine.Editor.Materials.Generator.Structs
{
    public struct Operation
    {
        public int Id;
        public string Name;
        public SType Type;
        public int Refs;

        public Operation(int id, string name, SType type)
        {
            Id = id;
            Name = name;
            Type = type;
            Refs = 0;
        }
    }
}