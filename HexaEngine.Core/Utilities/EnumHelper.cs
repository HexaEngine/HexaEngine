namespace HexaEngine.Core.Utilities
{
    using System;
    using System.Collections.Generic;

    public static class EnumHelper<T> where T : struct, Enum
    {
        private static readonly Dictionary<T, string> _nameDict = [];

        static EnumHelper()
        {
            T[] values = Enum.GetValues<T>();

            for (int i = 0; i < values.Length; i++)
            {
                T value = values[i];
                _nameDict.Add(value, value.ToString());
            }
        }

        public static string GetName(T value)
        {
            return _nameDict[value];
        }
    }
}