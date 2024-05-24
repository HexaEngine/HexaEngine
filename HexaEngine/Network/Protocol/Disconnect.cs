using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexaEngine.Network.Protocol
{
    public struct Disconnect : IRecord
    {
        public readonly RecordType Type => RecordType.Disconnect;

        public readonly void Free()
        {
        }

        public readonly int Read(ReadOnlySpan<byte> span)
        {
            return 0;
        }

        public readonly int SizeOf()
        {
            return 0;
        }

        public readonly int Write(Span<byte> span)
        {
            return 0;
        }
    }
}