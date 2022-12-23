namespace HexaEngine.Core.Events
{
    using HexaEngine.Core;

    public class RestoredEventArgs : RoutedEventArgs
    {
        public WindowState OldState { get; internal set; }

        public WindowState NewState { get; internal set; }
    }
}