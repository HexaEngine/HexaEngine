namespace HexaEngine.Editor.Projects
{
    using System.Collections.Generic;
    using System.IO;

    public static class ProjectHistory
    {
        private const string historyFile = "projectHistory.json";
        private static readonly string historyPath = Path.Combine(EditorConfig.BasePath, historyFile);
        private static readonly List<HistoryEntry> entries;

        static ProjectHistory()
        {
            if (File.Exists(historyPath))
            {
                entries = JsonConvert.DeserializeObject<List<HistoryEntry>>(File.ReadAllText(historyPath)) ?? new();
            }
            else
            {
                entries = new();
            }
        }

        public static IReadOnlyList<HistoryEntry> Entries => entries;

        public static void AddEntry(string name, string path)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.Path == path)
                {
                    entry.LastAccess = DateTime.UtcNow;
                    entries[i] = entry;
                    Save();
                    return;
                }
            }

            entries.Add(new HistoryEntry(name, path));
            Save();
        }

        public static void Pin(string path)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.Path == path)
                {
                    entry.Pinned = true;
                    entries[i] = entry;
                    Save();
                    return;
                }
            }
        }

        public static void Unpin(string path)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.Path == path)
                {
                    entry.Pinned = false;
                    entries[i] = entry;
                    Save();
                    return;
                }
            }
        }

        public static void RemoveEntryByName(string name)
        {
            entries.RemoveAll(x => x.Name == name);
            Save();
        }

        public static void RemoveEntryByPath(string path)
        {
            entries.RemoveAll(x => x.Path == path);
            Save();
        }

        public static void Clear()
        {
            entries.Clear();
            Save();
        }

        private static void Save()
        {
            File.WriteAllText(historyPath, JsonConvert.SerializeObject(entries));
        }
    }
}