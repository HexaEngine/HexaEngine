namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Numerics;

    public class ConsoleColorPalette
    {
        private readonly Vector4[] values;

        public ConsoleColorPalette()
        {
            values = new Vector4[Enum.GetValues<ConsoleColor>().Length];
            this[ConsoleColor.Black] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            this[ConsoleColor.DarkBlue] = new Vector4(0.0f, 0.0f, 0.5f, 1.0f);
            this[ConsoleColor.DarkGreen] = new Vector4(0.0f, 0.5f, 0.0f, 1.0f);
            this[ConsoleColor.DarkCyan] = new Vector4(0.0f, 0.5f, 0.5f, 1.0f);
            this[ConsoleColor.DarkRed] = new Vector4(0.5f, 0.0f, 0.0f, 1.0f);
            this[ConsoleColor.DarkMagenta] = new Vector4(0.5f, 0.0f, 0.5f, 1.0f);
            this[ConsoleColor.DarkYellow] = new Vector4(0.5f, 0.5f, 0.0f, 1.0f);
            this[ConsoleColor.Gray] = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
            this[ConsoleColor.DarkGray] = new Vector4(0.25f, 0.25f, 0.25f, 1.0f);
            this[ConsoleColor.Blue] = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
            this[ConsoleColor.Green] = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            this[ConsoleColor.Cyan] = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);
            this[ConsoleColor.Red] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this[ConsoleColor.Magenta] = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
            this[ConsoleColor.Yellow] = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            this[ConsoleColor.White] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }

        public ref Vector4 this[ConsoleColor index]
        {
            get { return ref values[(int)index]; }
        }
    }
}