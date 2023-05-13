namespace HexaEngine.SPIRVCross
{
    public static unsafe partial class SPIRV
    {
        static SPIRV()
        {
            LibraryLoader.SetImportResolver();
        }
    }
}