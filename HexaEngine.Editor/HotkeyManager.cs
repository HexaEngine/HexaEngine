namespace HexaEngine.Editor
{
    using HexaEngine.Core.Input;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public static class HotkeyManager
    {
        private static readonly List<Key> keys = new();
        private static readonly List<Hotkey> hotkeys = new();
        private const string Filename = "hotkeys.json";

        static HotkeyManager()
        {
            if (File.Exists(Filename))
            {
                JsonConvert.DeserializeObject(File.ReadAllText(Filename));
            }

            Keyboard.KeyDown += KeyboardPressed;
            Keyboard.KeyUp += KeyboardReleased;
        }

        public static object SyncObject => hotkeys;

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
                    {
                        return i;
                    }
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
                    {
                        return hotkey;
                    }
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
                if (TryFind(name, out var hotkey))
                {
                    hotkey.Callback = callback;
                }
                else
                {
                    hotkey = new(name, callback);
                    hotkeys.Add(hotkey);
                }

                Save();

                return hotkey;
            }
        }

        public static Hotkey Register(string name, Action callback, params Key[] defaults)
        {
            lock (hotkeys)
            {
                if (TryFind(name, out var hotkey))
                {
                    hotkey.Callback = callback;
                }
                else
                {
                    hotkey = new(name, callback, defaults);
                    hotkeys.Add(hotkey);
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
                {
                    return;
                }

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