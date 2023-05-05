namespace TestApp
{
    using System.Text;

    public static class EnumConverterGenerator
    {
        public static void Convert()
        {
            string fileA = File.ReadAllText("Format.cs");
            string fileB = File.ReadAllText("FormatSilk.cs");
            string[] linesA = fileA.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string[] linesB = fileB.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string namespaceA = "Format.";
            string namespaceB = "Silk.NET.DXGI.Format.";

            StringBuilder sbConvert = new();
            for (int i = 0; i < linesA.Length; i++)
            {
                sbConvert.AppendLine($"{namespaceA}{linesA[i]} => {namespaceB}{linesB[i]},");
            }
            StringBuilder sbConvertBack = new();
            for (int i = 0; i < linesA.Length; i++)
            {
                sbConvertBack.AppendLine($"{namespaceB}{linesB[i]} => {namespaceA}{linesA[i]},");
            }
            File.WriteAllText("convert.txt", sbConvert.ToString());
            File.WriteAllText("convertback.txt", sbConvertBack.ToString());
        }
    }
}