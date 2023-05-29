namespace HexaEngine.Editor.MaterialEditor.Generator.Structs
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using System.Text;

    public struct Operation
    {
        public int Id;
        public string Name;
        public SType Type;
        public string Definition;
        public int Refs;

        public Operation(int id, string name, SType type, string definition)
        {
            Id = id;
            Name = name;
            Type = type;
            Refs = 0;
            Definition = definition;
        }

        public void Append(StringBuilder builder)
        {
            if (!string.IsNullOrEmpty(Definition))
            {
                builder.AppendLine($"{Type.GetTypeName()} {Name} = {Definition};");
            }
        }

        public void AppendInline(StringBuilder builder)
        {
            if (!string.IsNullOrEmpty(Definition))
            {
                builder.Append(Definition);
            }
        }
    }
}