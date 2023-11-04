namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a palette of colors for a terminal with mapping to <see cref="TerminalColor"/> values.
    /// </summary>
    public class TerminalColorPalette
    {
        private readonly Vector4[] values;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalColorPalette"/> class.
        /// Sets default color values for all <see cref="TerminalColor"/> values.
        /// </summary>
        public TerminalColorPalette()
        {
            values = new Vector4[Enum.GetValues<TerminalColor>().Length];
            this[TerminalColor.Black] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            this[TerminalColor.DarkBlue] = new Vector4(0.0f, 0.0f, 0.5f, 1.0f);
            this[TerminalColor.DarkGreen] = new Vector4(0.0f, 0.5f, 0.0f, 1.0f);
            this[TerminalColor.DarkCyan] = new Vector4(0.0f, 0.5f, 0.5f, 1.0f);
            this[TerminalColor.DarkRed] = new Vector4(0.5f, 0.0f, 0.0f, 1.0f);
            this[TerminalColor.DarkMagenta] = new Vector4(0.5f, 0.0f, 0.5f, 1.0f);
            this[TerminalColor.DarkYellow] = new Vector4(0.5f, 0.5f, 0.0f, 1.0f);
            this[TerminalColor.Gray] = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
            this[TerminalColor.DarkGray] = new Vector4(0.25f, 0.25f, 0.25f, 1.0f);
            this[TerminalColor.Blue] = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
            this[TerminalColor.Green] = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            this[TerminalColor.Cyan] = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);
            this[TerminalColor.Red] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this[TerminalColor.Magenta] = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
            this[TerminalColor.Yellow] = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            this[TerminalColor.White] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }

        /// <summary>
        /// Gets or sets the color represented by the specified <see cref="TerminalColor"/>.
        /// </summary>
        /// <param name="index">The <see cref="TerminalColor"/> value.</param>
        /// <returns>Reference to the <see cref="Vector4"/> color value.</returns>
        public ref Vector4 this[TerminalColor index]
        {
            get { return ref values[(int)index]; }
        }
    }
}