namespace TestApp
{
    using HexaEngine.DirectXTex;

    public static partial class Program
    {
        public static void Main()
        {
            DirectXTex.BitsPerColor(Silk.NET.DXGI.Format.FormatR9G9B9E5Sharedexp);
            SPIRVCommandsRewriter.Rewrite();
        }
    }
}