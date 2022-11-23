namespace HexaEngine.Core.Input2
{
    public static class Helper
    {
        public static KeyCode Convert(Silk.NET.SDL.KeyCode code)
        {
            return (KeyCode)code;
        }

        public static Silk.NET.SDL.KeyCode ConvertBack(KeyCode code)
        {
            return (Silk.NET.SDL.KeyCode)code;
        }
    }
}