namespace HexaEngine.Editor.Projects
{
    using System.Collections.Generic;
    using System.IO;

    public static class ProjectHistory
    {
        private static readonly List<HistoryEntry> entries;

        static ProjectHistory()
        {
            if (File.Exists("projectHistory.json"))
            {
                entries = JsonConvert.DeserializeObject<List<HistoryEntry>>(File.ReadAllText("projectHistory.json")) ?? new();
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
                    entries.RemoveAt(i);
                    entries.Insert(0, entry);
                    Save();
                    return;
                }
            }

            entries.Add(new HistoryEntry { Name = name, Path = path });
            Save();
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
            File.WriteAllText("projectHistory.json", JsonConvert.SerializeObject(entries));
        }
    }
}