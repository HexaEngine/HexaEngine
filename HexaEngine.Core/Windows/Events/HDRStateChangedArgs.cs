namespace HexaEngine.Core.Windows.Events
{
    public class HDRStateChangedArgs : RoutedEventArgs
    {
        public bool HDREnabled { get; set; }

        public float SDRWhiteLevel { get; set; }

        public float HDRHeadroom { get; set; }
    }
}