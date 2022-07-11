namespace HexaEngine.Core.Logging
{
    using System.Text;

    public class DebugFormatter
    {
        public static string ToString<T>(T t, char sep = ' ')
        {
            var sb = new StringBuilder();
            var type = typeof(T);
            foreach (var prop in type.GetProperties())
            {
                _ = sb.Append($"{prop.Name}: {prop.GetValue(t)}{sep}");
            }
            foreach (var field in type.GetFields())
            {
                _ = sb.Append($"{field.Name}: {field.GetValue(t)}{sep}");
            }
            return sb.ToString();
        }
    }
}