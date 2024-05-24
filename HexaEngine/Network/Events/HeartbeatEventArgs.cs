namespace HexaEngine.Network.Events
{
    using HexaEngine.Network.Protocol;

    public struct HeartbeatEventArgs
    {
        public TimeSpan RoundTripTime;
        public TimeSpan TimeOffset;

        public HeartbeatEventArgs(Heartbeat heartbeat)
        {
            RoundTripTime = heartbeat.RoundTripTime;
            TimeOffset = heartbeat.RoundTripTime / 2;
        }

        public HeartbeatEventArgs(TimeSpan roundTripTime, TimeSpan timeOffset)
        {
            RoundTripTime = roundTripTime;
            TimeOffset = timeOffset;
        }
    }
}