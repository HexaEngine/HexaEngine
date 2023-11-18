namespace HexaEngine.Editor.Extensions
{
    using HexaEngine.Core.Input;
    using System.Collections.Generic;
    using System.Text;

    public static class KeyExtensions
    {
        public static string ToFormattedString(this IList<Key> keys)
        {
            StringBuilder sb = new();
            for (int i = 0; i < keys.Count; i++)
            {
                if (sb.Length > 0)
                {
                    sb.Append("+" + keys[i]);
                }
                else
                {
                    sb.Append(keys[i]);
                }
            }
            return sb.ToString();
        }
    }
}