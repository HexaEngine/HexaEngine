namespace HexaEngine.Editor.Projects
{
    public struct HistoryEntry
    {
        private string? cache;
        private string? lastAccessCache;

        public HistoryEntry(string name, string path)
        {
            Name = name;
            Path = path;
            Pinned = false;
            LastAccess = DateTime.UtcNow;
        }

        public HistoryEntry(string name, string path, bool pinned)
        {
            Name = name;
            Path = path;
            Pinned = pinned;
            LastAccess = DateTime.UtcNow;
        }

        [JsonConstructor]
        public HistoryEntry(string name, string path, bool pinned, DateTime lastAccess)
        {
            Name = name;
            Path = path;
            Pinned = pinned;
            LastAccess = lastAccess;
        }

        public string Name { get; set; }

        public string Path { get; set; }

        public bool Pinned { get; set; }

        public DateTime LastAccess { get; set; }

        public string LastAccessString => lastAccessCache ??= LastAccess.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

        [JsonIgnore]
        public string FullName => cache ??= $"{Name}, {Path}";
    }
}