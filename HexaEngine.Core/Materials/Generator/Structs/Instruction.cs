namespace HexaEngine.Materials.Generator.Structs
{
    using HexaEngine.Materials.Generator;

    public class Operation
    {
        public int Id;
        public string Name;
        public SType Type;
        public string Definition;
        public int RefCount;
        public bool AllowInline;
        public bool HasOutput;
        public List<Operation> References = new();

        public Operation(int id, string name, SType type, string definition, bool allowInline, bool hasOutput)
        {
            Id = id;
            Name = name;
            Type = type;
            RefCount = 0;
            Definition = definition;
            AllowInline = allowInline;
            HasOutput = hasOutput;
        }

        public bool CanInline => RefCount == 1 && AllowInline;

        public void Append(CodeWriter builder, bool inline)
        {
            if (!string.IsNullOrEmpty(Definition))
            {
                if (inline)
                {
                    for (int i = 0; i < References.Count; i++)
                    {
                        var reference = References[i];
                        if (reference.CanInline)
                        {
                            Definition = Definition.Replace(reference.Name, $"({reference.AppendInline()})");
                        }
                    }
                }
                if (HasOutput)
                {
                    builder.WriteLine($"{Type.GetTypeName()} {Name} = {Definition};");
                }
                else
                {
                    builder.WriteLine($"{Definition};");
                }
            }
        }

        public string AppendInline()
        {
            if (!string.IsNullOrEmpty(Definition))
            {
                for (int i = 0; i < References.Count; i++)
                {
                    var reference = References[i];
                    if (reference.CanInline)
                    {
                        Definition = Definition.Replace(reference.Name, $"({reference.AppendInline()})");
                    }
                }
                return Definition;
            }
            return string.Empty;
        }
    }
}