namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;
    using System;

    public static class ComboEnumHelper<T> where T : struct, Enum
    {
        private static readonly T[] values = Enum.GetValues<T>();
        private static readonly string[] names = Enum.GetNames<T>();

        public static bool Combo(string label, ref T value)
        {
            int index = Array.IndexOf(values, value);
            if (ImGui.Combo(label, ref index, names, names.Length))
            {
                value = values[index];
                return true;
            }
            return false;
        }

        public static void Text(T value)
        {
            int index = Array.IndexOf(values, value);
            ImGui.Text(names[index]);
        }
    }

    public static class ComboEnumHelper
    {
        private static readonly Dictionary<Type, object[]> values = new();
        private static readonly Dictionary<Type, string[]> names = new();

        private static void Get(Type type, out object[] values, out string[] names)
        {
            if (ComboEnumHelper.values.TryGetValue(type, out var objects))
            {
                values = objects;
                names = ComboEnumHelper.names[type];
                return;
            }

            values = Enum.GetValues(type).Cast<object>().ToArray();
            names = Enum.GetNames(type);
            ComboEnumHelper.values.Add(type, values);
            ComboEnumHelper.names.Add(type, names);
        }

        public static bool Combo(string label, Type type, ref object value)
        {
            Get(type, out var values, out var names);
            int index = Array.IndexOf(values, value);
            if (ImGui.Combo(label, ref index, names, names.Length))
            {
                value = values[index];
                return true;
            }
            return false;
        }

        public static void Text(Type type, object value)
        {
            Get(type, out var values, out var names);
            int index = Array.IndexOf(values, value);
            ImGui.Text(names[index]);
        }
    }
}