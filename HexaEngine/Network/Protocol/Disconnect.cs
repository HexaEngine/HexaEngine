using Hexa.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexaEngine.Network.Protocol
{
    [ProtobufRecord]
    public partial struct Disconnect : IRecord
    {
        public readonly RecordType Type => RecordType.Disconnect;
    }
}