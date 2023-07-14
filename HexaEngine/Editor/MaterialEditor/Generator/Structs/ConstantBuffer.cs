namespace HexaEngine.Editor.MaterialEditor.Generator.Structs
{
    using System.Globalization;
    using System.Text;

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
            using (builder.PushBlockSemicolon($"cbuffer {Name} : register(b{Slot.ToString(CultureInfo.InvariantCulture)})"))
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