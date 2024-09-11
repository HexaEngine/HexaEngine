namespace HexaEngine.Materials.Generator.Structs
{
    using HexaEngine.Materials.Generator;

    public struct ConstantBuffer
    {
        public uint Slot;
        public string Name;
        public List<ConstantBufferDef> Defs = new();

        public ConstantBuffer(uint slot, string name) : this()
        {
            Slot = slot;
            Name = name;
        }

        public void Build(CodeWriter builder)
        {
            using (builder.PushBlockSemicolon($"cbuffer {Name}"))
            {
                for (int i = 0; i < Defs.Count; i++)
                {
                    ConstantBufferDef def = Defs[i];
                    builder.WriteLine($"{def.Type.GetTypeName()} {def.Name};");
                }
            }
        }
    }
}