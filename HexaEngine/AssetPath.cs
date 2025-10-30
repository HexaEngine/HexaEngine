namespace HexaEngine
{
    internal class AssetPathHelper
    {
        public static string AssetShaderPath(string relativePath)
        {
            return $"HexaEngine.Core:shaders/{relativePath}";
        }
    }
}