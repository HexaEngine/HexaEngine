namespace HexaEngine.Editor.Projects
{
    public class PropertyGroupItem
    {
        public PropertyGroupItem(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}