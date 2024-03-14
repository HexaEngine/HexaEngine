namespace HexaEngine.UI.Graphics
{
    using HexaEngine.Core;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;

    public static class UIFactory
    {
        private static readonly List<IDisposable> resources = [];
        private static readonly object _lock = new();

        public static SolidColorBrush CreateSolidColorBrush(Color color)
        {
            SolidColorBrush brush = new(Application.GraphicsDevice, color);
            lock (_lock)
            {
                resources.Add(brush);
            }
            return brush;
        }

        public static readonly SolidColorBrush Transparent = CreateSolidColorBrush(Colors.Transparent);

        public static void DisposeResources()
        {
            for (int i = 0; i < resources.Count; i++)
            {
                resources[i].Dispose();
            }
        }
    }
}