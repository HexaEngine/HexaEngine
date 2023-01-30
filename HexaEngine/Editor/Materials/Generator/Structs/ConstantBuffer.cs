namespace HexaEngine.Editor.Materials.Generator.Structs
{
    using System.Globalization;
    using System.Text;

    public struct ConstantBuffer
    {
        public int Slot;
        public string Name;
        public List<ConstantBufferDef> Defs = new();

        public ConstantBuffer(int slot, string name) : this()
        {
            Slot = slot;
            Name = name;
        }

        public void Build(StringBuilder builder)
        {
            builder.AppendLine($"cbuffer {Name} : register(b{Slot.ToString(CultureInfo.InvariantCulture)})");
            builder.AppendLine("{");
            for (int i = 0; i < Defs.Count; i++)
            {
                ConstantBufferDef def = Defs[i];
                builder.AppendLine($"{def.Type.GetTypeName()} {def.Name};");
            }
            builder.AppendLine("};");
        }
    }
}