namespace HexaEngine.Core.Events
{
    using HexaEngine.Core;

    public class RestoredEventArgs : RoutedEventArgs
    {
        public RestoredEventArgs(WindowState oldState, WindowState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public WindowState OldState { get; }

        public WindowState NewState { get; }
    }
}