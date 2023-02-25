namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Input;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class KeyExtensions
    {
        public static string ToFormattedString(this IList<KeyCode> keys)
        {
            StringBuilder sb = new();
            for (int i = 0; i < keys.Count; i++)
            {
                if (sb.Length > 0)
                    sb.Append("+" + keys[i]);
                else
                    sb.Append(keys[i]);
            }
            return sb.ToString();
        }
    }

    public class Hotkey
    {
        private string? tostring;
        private readonly List<KeyCode> keys = new();
        private readonly List<KeyCode> defaults = new();
        public readonly string Name;

        [JsonIgnore]
        public Action Callback;

        private ConfigValue? confValue;

        [JsonIgnore]
        internal ConfigValue? Value
        {
            get => confValue; set
            {
                if (value == null)
                {
                    return;
                }
                confValue = value;
                confValue.DefaultValue = defaults.ToFormattedString();
                confValue.ValueChanged += ValueChanged;
                keys.Clear();
                keys.AddRange(value.GetKeys());
            }
        }

        public ObservableWrapper<KeyCode> Keys { get; private set; }

        public List<KeyCode> Defaults => defaults;

        public Hotkey(string name, Action callback)
        {
            Name = name;
            Callback = callback;
            keys = new();
            Keys = new(keys);
            Keys.CollectionChanged += CollectionChanged;
        }

        public Hotkey(string name, Action callback, List<KeyCode> defaults)
        {
            Name = name;
            Callback = callback;
            this.defaults.AddRange(defaults);
            keys = defaults;
            Keys = new(keys);
            Keys.CollectionChanged += CollectionChanged;
        }

        public Hotkey(string name, Action callback, IEnumerable<KeyCode> defaults)
        {
            Name = name;
            Callback = callback;
            this.defaults.AddRange(defaults);
            keys = new(defaults);
            Keys = new(keys);
            Keys.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (confValue == null)
                return;
            tostring = null;
            confValue.Value = ToString();
        }

        private void ValueChanged(ConfigValue v, string? a)
        {
            keys.Clear();
            keys.AddRange(v.GetKeys());
        }

        public bool IsConflicting(Hotkey other)
        {
            if (Keys.Count != other.Keys.Count)
                return false;
            for (int i = 0; i < Keys.Count; i++)
            {
                if (Keys[i] != other.Keys[i])
                    return false;
            }
            return true;
        }

        public bool IsSet => Keys.Count != 0;

        public bool CanExecute()
        {
            return Keys.Count > 0;
        }

        public bool TryExecute(List<KeyCode> keys)
        {
            if (keys.Count < Keys.Count)
                return false;

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
            if (tostring == null)
            {
                StringBuilder sb = new();
                for (int i = 0; i < Keys.Count; i++)
                {
                    if (sb.Length > 0)
                        sb.Append("+" + Keys[i]);
                    else
                        sb.Append(Keys[i]);
                }
                tostring = sb.ToString();
            }
            return tostring;
        }
    }
}