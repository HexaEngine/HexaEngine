﻿namespace IBLBaker
{
    using HexaEngine.Core.Graphics;
    using System.Reflection;

    public static class WidgetManager
    {
        private static List<Type> widgetTypes = new();
        private static List<Widget> widgets = new();

        static WidgetManager()
        {
            widgetTypes.AddRange(Assembly.GetExecutingAssembly().GetTypes().FilterFor<Widget>());
        }

        private static IEnumerable<Type> FilterFor<T>(this Type[] types)
        {
            var type = typeof(T);
            return types.AsParallel().Where(x => x.IsAssignableTo(type) && !x.IsAbstract);
        }

        public static void Init(IGraphicsDevice device)
        {
            for (int i = 0; i < widgetTypes.Count; i++)
            {
                var widget = (Widget?)Activator.CreateInstance(widgetTypes[i], device);
                if (widget is not null)
                    widgets.Add(widget);
            }
        }

        public static void Draw(IGraphicsContext context)
        {
            for (int i = 0; i < widgets.Count; i++)
            {
                widgets[i].Draw(context);
            }
        }

        public static void Dispose()
        {
            for (int i = 0; i < widgets.Count; i++)
            {
                widgets[i].Dispose();
            }
            widgets.Clear();
        }
    }
}