namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a palette of colors for console text based on ConsoleColor values.
    /// </summary>
    public class ConsoleColorPalette
    {
        private readonly Vector4[] values;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleColorPalette"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the color associated with the specified <see cref="ConsoleColor"/>.
        /// </summary>
        /// <param name="index">The <see cref="ConsoleColor"/> value for which to get or set the color.</param>
        /// <returns>A reference to the <see cref="Vector4"/> color value associated with the specified <see cref="ConsoleColor"/>.</returns>
        public ref Vector4 this[ConsoleColor index]
        {
            get { return ref values[(int)index]; }
        }
    }
}