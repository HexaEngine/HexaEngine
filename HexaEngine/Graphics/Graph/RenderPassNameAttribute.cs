namespace HexaEngine.Graphics.Graph
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RenderPassNameAttribute : Attribute
    {
        public RenderPassNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
