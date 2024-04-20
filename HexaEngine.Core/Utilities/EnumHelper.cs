namespace HexaEngine.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Field)]
    public class EnumNameAttribute : Attribute
    {
        public EnumNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public static class EnumHelper<T> where T : struct, Enum
    {
        private static readonly Dictionary<T, string> _nameDict = [];

        static EnumHelper()
        {
            T[] values = Enum.GetValues<T>();
            Type enumType = typeof(T);

            for (int i = 0; i < values.Length; i++)
            {
                var value = values[i];
                var name = value.ToString();
                var field = enumType.GetField(name);
                var attribute = field?.GetCustomAttribute<EnumNameAttribute>();

                if (attribute != null)
                {
                    name = attribute.Name;
                }

                _nameDict[value] = name;
            }
        }

        public static string GetName(T value)
        {
            return _nameDict[value];
        }
    }
}