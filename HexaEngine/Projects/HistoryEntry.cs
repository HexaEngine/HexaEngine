namespace HexaEngine.Projects
{
    public struct HistoryEntry
    {
        public string Name { get; set; }
        public string Path { get; set; }

        [JsonIgnore]
        public string Fullname => $"{Name}, {Path}";
    }
}