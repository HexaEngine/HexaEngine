﻿namespace HexaEngine.Projects
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

        internal static void AddEntry(string name, string path)
        {
            if (entries.Any(entry => entry.Path == path))
            {
                return;
            }

            entries.Add(new HistoryEntry { Name = name, Path = path });
            Save();
        }

        internal static void RemoveEntryByName(string name)
        {
            entries.RemoveAll(x => x.Name == name);
            Save();
        }

        internal static void RemoveEntryByPath(string path)
        {
            entries.RemoveAll(x => x.Path == path);
            Save();
        }

        internal static void Clear()
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