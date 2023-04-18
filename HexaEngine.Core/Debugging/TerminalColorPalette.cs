namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Numerics;

    public class TerminalColorPalette
    {
        private readonly Vector4[] values;

        public TerminalColorPalette()
        {
            values = new Vector4[Enum.GetValues<TerminalColor>().Length];
            this[TerminalColor.Text] = new(1.0f, 1.0f, 1.0f, 1f);
            this[TerminalColor.Warning] = new(1.0f, 0.87f, 0.37f, 1.0f);
            this[TerminalColor.Error] = new(1.0f, 0.365f, 0.365f, 1.0f);
        }

        public ref Vector4 this[TerminalColor index]
        {
            get { return ref values[(int)index]; }
        }
    }
}