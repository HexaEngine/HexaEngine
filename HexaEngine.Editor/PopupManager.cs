namespace HexaEngine.Editor
{
    using HexaEngine.Editor.Dialogs;
    using System.Collections.Generic;

    public static class PopupManager
    {
        private static readonly List<IPopup> popups = new();
        private static readonly object _lock = new();

        public static object SyncLock => _lock;

        public static IReadOnlyList<IPopup> Popups => popups;

        public static void Show(IPopup popup)
        {
            lock (_lock)
            {
                popups.Add(popup);
                popup.Show();
            }
        }

        public static void Show<T>() where T : IPopup, new()
        {
            T popup = new();
            lock (_lock)
            {
                popups.Add(popup);
                popup.Show();
            }
        }

        public static void Close(IPopup popup)
        {
            lock (_lock)
            {
                popup.Close();
                popups.Remove(popup);
            }
        }

        public static void Draw()
        {
            lock (_lock)
            {
                for (var i = 0; i < popups.Count; i++)
                {
                    var popup = popups[i];
                    if (!popup.Shown)
                    {
                        popups.RemoveAt(i);
                        i--;
                        continue;
                    }
                    popup.Draw();
                }
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                for (int i = 0; i < popups.Count; i++)
                {
                    popups[i].Close();
                }
                popups.Clear();
            }
        }

        public static void Dispose()
        {
            Clear();
        }
    }
}