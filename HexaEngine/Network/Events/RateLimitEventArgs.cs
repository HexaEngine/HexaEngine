namespace HexaEngine.Network.Events
{
    using HexaEngine.Network.Protocol;

    public struct RateLimitEventArgs
    {
        public TimeSpan TimeOffset;
        public DateTime Timestamp;
        public DateTime RateLimitReset;
        public DateTime LocalRateLimitReset;
        public byte Warning;

        public RateLimitEventArgs(HexaProtoClient client, RateLimit rateLimit, DateTime now)
        {
            TimeOffset = client.TimeOffset;
            Timestamp = rateLimit.Timestamp;
            RateLimitReset = rateLimit.RateLimitReset;
            LocalRateLimitReset = now + (RateLimitReset - Timestamp) + TimeOffset;
            Warning = rateLimit.Warning;
        }

        public RateLimitEventArgs(TimeSpan timeOffset, DateTime timestamp, DateTime rateLimitReset, DateTime localRateLimitReset, byte warning)
        {
            TimeOffset = timeOffset;
            Timestamp = timestamp;
            RateLimitReset = rateLimitReset;
            LocalRateLimitReset = localRateLimitReset;
            Warning = warning;
        }
    }
}