namespace HexaEngine.Editor.Projects
{
    public struct HistoryEntry
    {
        private string? cache;

        public string Name { get; set; }
        public string Path { get; set; }

        [JsonIgnore]
        public string Fullname => cache ??= $"{Name}, {Path}";
    }
}