namespace HexaEngine.Editor.Materials
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
                builder.AppendLine($"{VariableTable.GetTypeName(def.Type)} {def.Name};");
            }
            builder.AppendLine("};");
        }
    }
}