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
            this[ConsoleColor.Command] = new(1.0f, 1.0f, 1.0f, 1.0f);
            this[ConsoleColor.Timestamp] = new(1.0f, 1.0f, 1.0f, 0.5f);
            this[ConsoleColor.Trace] = new(0.46f, 0.96f, 0.46f, 1.0f);
            this[ConsoleColor.Debug] = new(0.46f, 0.96f, 0.46f, 1.0f);
            this[ConsoleColor.Information] = new(1.0f, 1.0f, 1.0f, 1.0f);
            this[ConsoleColor.Warning] = new(1.0f, 0.87f, 0.37f, 1.0f);
            this[ConsoleColor.Error] = new(1.0f, 0.365f, 0.365f, 1.0f);
            this[ConsoleColor.Critical] = new(1.0f, 0.0f, 0.0f, 1.0f);
        }

        public ref Vector4 this[ConsoleColor index]
        {
            get { return ref values[(int)index]; }
        }
    }
}