namespace HexaEngine.Materials.Generator.Structs
{
    using HexaEngine.Materials.Generator;

    public struct ConstantBuffer
    {
        public uint Slot;
        public string Name;
        public List<ConstantBufferDef> Defs;

        public ConstantBuffer(uint slot, string name)
        {
            Slot = slot;
            Name = name;
            Defs = [];
        }

        public ConstantBuffer(uint slot, string name, List<ConstantBufferDef> defs)
        {
            Slot = slot;
            Name = name;
            Defs = defs;
        }

        public readonly void Build(CodeWriter builder)
        {
            string header = Slot == uint.MaxValue ? $"cbuffer {Name}" : $"cbuffer {Name} : register(b{Slot})";
            using (builder.PushBlockSemicolon(header))
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