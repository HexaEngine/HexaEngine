namespace HexaEngine.Core.Events
{
    public class MovedEventArgs : RoutedEventArgs
    {
        public MovedEventArgs(int oldX, int oldY, int newX, int newY)
        {
            OldX = oldX;
            OldY = oldY;
            NewX = newX;
            NewY = newY;
        }

        public int OldX { get; }

        public int OldY { get; }

        public int NewX { get; }

        public int NewY { get; }
    }
}