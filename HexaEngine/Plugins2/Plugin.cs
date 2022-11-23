namespace HexaEngine.Plugins2
{
    using System.Collections.Generic;

    public class Plugin
    {
        private string name;
        private string description;
        private readonly List<Plugin> dependencies = new();
        private readonly List<Record> records = new();
        private readonly List<Resource> resources = new();

        public Plugin(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get => name; set => name = value; }

        public string Description { get => description; set => description = value; }

        public List<Plugin> Dependencies => dependencies;

        public List<Record> Records => records;

        public List<Resource> Resources => resources;
    }
}