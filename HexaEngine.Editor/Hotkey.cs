namespace HexaEngine.Editor
{
    using HexaEngine.Core.Input;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    public class Hotkey
    {
        private string? cache;
        private readonly List<Key> keys = new();
        private readonly List<Key> defaults = new();
        private readonly HashSet<Hotkey> conflicts = new();
        public readonly string Name;

        [JsonIgnore]
        public bool Enabled { get; set; } = true;

        [JsonIgnore]
        public Action Callback;

        [JsonIgnore]
        public HashSet<Hotkey> Conflicts => conflicts;

        public List<Key> Keys { get; private set; }

        public List<Key> Defaults => defaults;

        public Hotkey(string name, Action callback)
        {
            Name = name;
            Callback = callback;
            keys = new();
            Keys = new(keys);
        }

        public Hotkey(string name, Action callback, List<Key> defaults)
        {
            Name = name;
            Callback = callback;
            this.defaults.AddRange(defaults);
            keys = defaults;
            Keys = new(keys);
        }

        public Hotkey(string name, Action callback, IEnumerable<Key> defaults)
        {
            Name = name;
            Callback = callback;
            this.defaults.AddRange(defaults);
            keys = new(defaults);
            Keys = new(keys);
        }

        public void AddConflictingHotkey(Hotkey hotkey)
        {
            conflicts.Add(hotkey);
            hotkey.conflicts.Add(this);
        }

        public void RemoveConflictingHotkey(Hotkey hotkey)
        {
            conflicts.Remove(hotkey);
            hotkey.conflicts.Remove(this);
        }

        public bool IsConflicting(Hotkey other, bool useHashSet = true)
        {
            if (useHashSet)
            {
                return conflicts.Contains(other);
            }

            if (Keys.Count != other.Keys.Count)
            {
                return false;
            }

            for (int i = 0; i < Keys.Count; i++)
            {
                if (Keys[i] != other.Keys[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsSet => Keys.Count != 0;

        public bool CanExecute()
        {
            return Enabled && Keys.Count > 0;
        }

        public bool TryExecute(List<Key> keys)
        {
            if (keys.Count < Keys.Count)
            {
                return false;
            }

            for (int i = 0; i < Keys.Count; i++)
            {
                if (keys[i] != Keys[i])
                {
                    return false;
                }
            }
            Callback();
            return true;
        }

        public override string ToString()
        {
            if (cache == null)
            {
                StringBuilder sb = new();
                for (int i = 0; i < Keys.Count; i++)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("+" + Keys[i]);
                    }
                    else
                    {
                        sb.Append(Keys[i]);
                    }
                }
                cache = sb.ToString();
            }
            return cache;
        }

        public void Clear()
        {
            Keys.Clear();
            cache = null;
        }

        public void Add(Key key)
        {
            if (Keys.Contains(key))
            {
                return;
            }

            Keys.Add(key);
            cache = null;
        }

        public void AddRange(IEnumerable<Key> keys)
        {
            foreach (var key in keys)
            {
                Keys.Add(key);
            }
            cache = null;
        }

        public void SetToDefault()
        {
            Clear();
            Keys.AddRange(defaults);
        }
    }
}