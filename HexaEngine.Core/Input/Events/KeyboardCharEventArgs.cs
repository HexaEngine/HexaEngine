namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Windows.Events;

    public class KeyboardCharEventArgs : RoutedEventArgs
    {
        public KeyboardCharEventArgs()
        {
        }

        public char Char { get; internal set; }
    }
}