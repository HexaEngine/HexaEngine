namespace HexaEngine.Materials.Generator
{
    public struct Method
    {
        public string Name;
        public string Signature;
        public string ReturnType;
        public string Body;

        public void Build(CodeWriter builder)
        {
            using (builder.PushBlock($"{ReturnType} {Name}({Signature})"))
            {
                string[] lines = Body.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    builder.WriteLine(lines[i]);
                }
            }
            builder.WriteLine();
        }
    }
}