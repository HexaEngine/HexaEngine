namespace HexaEngine.Core.Input2.Events
{
    public class KeyboardCharEventArgs : EventArgs
    {
        public KeyboardCharEventArgs()
        {
        }

        public KeyboardCharEventArgs(char @char)
        {
            Char = @char;
        }

        public char Char;
    }
}