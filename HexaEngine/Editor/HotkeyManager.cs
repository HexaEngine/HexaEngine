namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Input;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;

    public static class HotkeyManager
    {
        private static readonly List<KeyCode> keys = new();
        private static readonly List<Hotkey> hotkeys = new();
        private static readonly ConfigKey config;
        private const string Filename = "hotkeys.json";

        static HotkeyManager()
        {
            config = Config.Global.GetOrCreateKey("Hotkeys");
            if (File.Exists(Filename))
                JsonConvert.DeserializeObject(File.ReadAllText(Filename));
            Keyboard.Pressed += KeyboardPressed;
            Keyboard.Released += KeyboardReleased;
        }

        public static IReadOnlyList<Hotkey> Hotkeys => hotkeys;

        public static int Count => hotkeys.Count;

        public static void Save()
        {
            lock (hotkeys)
            {
                File.WriteAllText(Filename, JsonConvert.SerializeObject(hotkeys));
            }
        }

        public static int IndexOf(string name)
        {
            lock (hotkeys)
            {
                for (int i = 0; i < hotkeys.Count; i++)
                {
                    if (hotkeys[i].Name == name)
                        return i;
                }
            }

            return -1;
        }

        public static Hotkey? Find(string name)
        {
            lock (hotkeys)
            {
                for (int i = 0; i < hotkeys.Count; i++)
                {
                    var hotkey = hotkeys[i];
                    if (hotkey.Name == name)
                        return hotkey;
                }
            }

            return null;
        }

        public static bool TryFind(string name, [NotNullWhen(true)] out Hotkey? hotkey)
        {
            hotkey = Find(name);
            return hotkey != null;
        }

        public static Hotkey Register(string name, Action callback)
        {
            lock (hotkeys)
            {
                config.TryGetOrAddKeyValue(name, "", DataType.Keys, false, out var value);
                if (TryFind(name, out var hotkey))
                {
                    hotkey.Callback = callback;
                    hotkey.Value = value;
                }
                else
                {
                    hotkey = new(name, callback);
                    hotkeys.Add(hotkey);
                    hotkey.Value = value;
                }

                Save();

                return hotkey;
            }
        }

        public static Hotkey Register(string name, Action callback, params KeyCode[] defaults)
        {
            lock (hotkeys)
            {
                config.TryGetOrAddKeyValue(name, "", DataType.Keys, false, out var value);
                if (TryFind(name, out var hotkey))
                {
                    hotkey.Callback = callback;
                    hotkey.Value = value;
                }
                else
                {
                    hotkey = new(name, callback, defaults);
                    hotkeys.Add(hotkey);
                    hotkey.Value = value;
                }

                Save();

                return hotkey;
            }
        }

        public static void Unregister(string name)
        {
            lock (hotkeys)
            {
                var index = IndexOf(name);
                if (index == -1)
                    return;

                hotkeys.RemoveAt(index);
            }

            Save();
        }

        private static void KeyboardPressed(object? sender, Core.Input.Events.KeyboardEventArgs e)
        {
            keys.Add(e.KeyCode);
            lock (hotkeys)
            {
                for (int i = 0; i < hotkeys.Count; i++)
                {
                    var hotkey = hotkeys[i];
                    if (!hotkey.CanExecute())
                    {
                        continue;
                    }
                    if (hotkey.TryExecute(keys))
                    {
                        return;
                    }
                }
            }
        }

        private static void KeyboardReleased(object? sender, Core.Input.Events.KeyboardEventArgs e)
        {
            keys.Remove(e.KeyCode);
        }
    }
}