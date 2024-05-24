namespace HexaEngine.Network.Protocol
{
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    public enum HeartbeatKind : byte
    {
        Initial,
        FirstTrip,
        LastTrip,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Heartbeat : IRecord
    {
        [FieldOffset(0)]
        public HeartbeatKind Kind;

        [FieldOffset(1)]
        public DateTime Timestamp;

        [FieldOffset(9)]
        public TimeSpan PropagationDelay;

        [FieldOffset(1)]
        public TimeSpan RoundTripTime;

        public static Heartbeat CreateInitial()
        {
            return new()
            {
                Kind = HeartbeatKind.Initial,
                Timestamp = DateTime.UtcNow,
            };
        }

        public static Heartbeat CreateFirstTrip(DateTime timestamp, TimeSpan tripTime)
        {
            return new()
            {
                Kind = HeartbeatKind.FirstTrip,
                Timestamp = timestamp,
                PropagationDelay = tripTime,
            };
        }

        public static Heartbeat CreateLast(TimeSpan rtt)
        {
            return new()
            {
                Kind = HeartbeatKind.LastTrip,
                RoundTripTime = rtt,
            };
        }

        public readonly RecordType Type => RecordType.Heartbeat;

        public readonly int Write(Span<byte> span)
        {
            span[0] = (byte)Kind;
            switch (Kind)
            {
                case HeartbeatKind.Initial:
                    BinaryPrimitives.WriteInt64LittleEndian(span[1..], Timestamp.Ticks);
                    return 9;

                case HeartbeatKind.FirstTrip:
                    BinaryPrimitives.WriteInt64LittleEndian(span[1..], Timestamp.Ticks);
                    BinaryPrimitives.WriteInt64LittleEndian(span[9..], PropagationDelay.Ticks);
                    return 17;

                case HeartbeatKind.LastTrip:
                    BinaryPrimitives.WriteInt64LittleEndian(span[1..], RoundTripTime.Ticks);
                    return 9;

                default:
                    return 1;
            }
        }

        public int Read(ReadOnlySpan<byte> span)
        {
            Kind = (HeartbeatKind)span[0];
            switch (Kind)
            {
                case HeartbeatKind.Initial:
                    Timestamp = new(BinaryPrimitives.ReadInt64LittleEndian(span[1..]), DateTimeKind.Utc);
                    return 9;

                case HeartbeatKind.FirstTrip:
                    Timestamp = new(BinaryPrimitives.ReadInt64LittleEndian(span[1..]), DateTimeKind.Utc);
                    PropagationDelay = new(BinaryPrimitives.ReadInt64LittleEndian(span[9..]));
                    return 17;

                case HeartbeatKind.LastTrip:
                    RoundTripTime = new(BinaryPrimitives.ReadInt64LittleEndian(span[1..]));
                    return 9;

                default:
                    return 1;
            }
        }

        public readonly int SizeOf()
        {
            return Kind switch
            {
                HeartbeatKind.Initial => 9,
                HeartbeatKind.FirstTrip => 17,
                HeartbeatKind.LastTrip => 9,
                _ => 1,
            };
        }

        public readonly void Free()
        {
        }
    }
}