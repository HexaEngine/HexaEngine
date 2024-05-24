namespace HexaEngine.Network
{
    using HexaEngine.Network.Events;
    using HexaEngine.Network.Protocol;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;

    public abstract class HexaProtoClientBase : BaseSocket
    {
        private unsafe byte* payloadReceiveBuffer = null;
        private uint payloadReceiveBufferLength = 0;

        private unsafe byte* payloadSendBuffer = null;
        private uint payloadSendBufferLength = 0;

        private readonly Memory<byte> receiveBuffer = new byte[8192];
        private int bufferStart = 0;

        private int receivedBytes = 0; // Track the number of bytes received for the current record
        private bool readContainer = false;
        private Container container;
        private Record record;
        private bool readHeader = false; // Flag to indicate if the record header has been read
        private int state;

        private Memory<byte> sendBuffer = new byte[8192];

        private readonly List<IRecord> queue = [];

        protected HexaProtoClientBase(Socket socket) : base(socket)
        {
        }

        protected HexaProtoClientBase(AddressFamily family, SocketType type, ProtocolType protocol) : base(family, type, protocol)
        {
        }

        public uint PayloadLimit { get; set; } = 1024;

        public uint RateLimit { get; set; } = 1000;

        public uint HeartbeatRate { get; set; } = 5000;

        public ProtocolErrorEventHandler? ProtocolError;

        public delegate void ProtocolErrorEventHandler(HexaProtoClientBase sender, ProtocolErrorEventArgs error);

        protected virtual void OnProtocolError(ProtocolError error)
        {
            ProtocolError?.Invoke(this, new(error));
        }

        public unsafe void Receive()
        {
            int received = Receive(receiveBuffer[bufferStart..]);

            received += bufferStart;

            // If the receive buffer is empty, early exit.
            if (received == 0)
            {
                return;
            }
            int read = 0;
            if (!readContainer)
            {
                // Skip initialization of the container for performance reasons
                Unsafe.SkipInit(out container);
                if (container.TryRead(receiveBuffer, 0, received, out read))
                {
                    readContainer = true;
                }
            }

            if (readContainer)
            {
                if (!readHeader)
                {
                    // Skip initialization of the record for performance reasons
                    Unsafe.SkipInit(out record);
                }

                if (ProcessSegment(receiveBuffer, received, ref read, ref record, ref receivedBytes, ref readHeader, out var error))
                {
                    receivedBytes = 0;
                    readHeader = false;
                    state++;
                }

                if (error)
                {
                    return;
                }

                if (state == container.NumRecords)
                {
                    state = 0;
                    readContainer = false;
                }
            }

            SetBufferPosition(receiveBuffer, read, received - read);
        }

        public async ValueTask ReceiveAsync(CancellationToken token)
        {
            // Receive data from the socket into the receiveBuffer
            int received = await ReceiveAsync(receiveBuffer[bufferStart..], token);

            received += bufferStart;

            // If the receive buffer is empty, early exit.
            if (received == 0)
            {
                return;
            }

            int read = 0;
            if (!readContainer)
            {
                // Skip initialization of the container for performance reasons
                Unsafe.SkipInit(out container);
                if (container.TryRead(receiveBuffer, 0, received, out read))
                {
                    readContainer = true;
                }
            }

            if (readContainer)
            {
                if (!readHeader)
                {
                    // Skip initialization of the record for performance reasons
                    Unsafe.SkipInit(out record);
                }

                if (ProcessSegment(receiveBuffer, received, ref read, ref record, ref receivedBytes, ref readHeader, out bool error))
                {
                    receivedBytes = 0;
                    readHeader = false;
                    state++;
                }

                if (error)
                {
                    return;
                }

                if (state == container.NumRecords)
                {
                    state = 0;
                    readContainer = false;
                }
            }

            SetBufferPosition(receiveBuffer, read, received - read);
        }

        private unsafe bool ProcessSegment(Memory<byte> buffer, int received, ref int read, ref Record record, ref int receivedBytes, ref bool readHeader, out bool error)
        {
            error = false;
            Span<byte> receiveBuffer = buffer.Span[..received];

            // Try to read the header of the record if it hasn't been read yet
            if (!readHeader && record.TryRead(receiveBuffer[read..], out var readBytes))
            {
                read += (int)readBytes; // Update the read position
                readHeader = true; // Mark the header as read

                if (record.Length > PayloadLimit)
                {
                    ProtocolError protocolError = new(ErrorCode.PayloadTooLarge, ErrorSeverity.Fatal);
                    Send(protocolError);
                    OnProtocolError(protocolError);
                    error = true;
                    return false;
                }

                // Check if the payload buffer needs to be reallocated
                if (record.Length > payloadReceiveBufferLength)
                {
                    payloadReceiveBuffer = (byte*)ReAlloc(payloadReceiveBuffer, record.Length); // Reallocate the payload buffer
                    payloadReceiveBufferLength = record.Length; // Update the payload buffer length
                }

                // Assign the payload buffer to the record
                record.Payload = payloadReceiveBuffer;
            }

            // If the header has been read, proceed to read the payload
            if (readHeader)
            {
                // Determine the length to copy based on the remaining buffer length and the record length
                int lengthToCopy = Math.Min(receiveBuffer.Length - read, (int)record.Length - receivedBytes);
                Span<byte> data = receiveBuffer.Slice(read, lengthToCopy); // Slice the buffer to get the data segment

                data.CopyTo(record.AsSpan()[receivedBytes..]); // Copy the data to the record

                receivedBytes += lengthToCopy; // Update the number of received bytes
                read += lengthToCopy; // Update the read position

                // If the entire record has been received, process it
                if (receivedBytes == record.Length)
                {
                    Process(record); // Process the record
                    return true; // Exit the loop for this record
                }
            }

            return false;
        }

        protected void SetBufferPosition(Memory<byte> buffer, int read, int left)
        {
            if (left == 0)
            {
                bufferStart = 0;
                return;
            }

            buffer.Slice(read, left).CopyTo(buffer);
            bufferStart = left;
        }

        public virtual unsafe void Send<T>(T record) where T : IRecord
        {
            Unsafe.SkipInit(out Container container);
            container.NumRecords = 1;
            container.Version = ProtocolVersion.Version1;

            uint bufferSize = container.SizeOf() + (uint)(Record.Size * container.NumRecords);

            bufferSize += (uint)record.SizeOf();

            if (bufferSize > payloadSendBufferLength)
            {
                payloadSendBuffer = (byte*)ReAlloc(payloadSendBuffer, bufferSize);
                payloadSendBufferLength = bufferSize;
            }

            Span<byte> buffer = new(payloadSendBuffer, (int)bufferSize);

            int idx = container.Write(buffer);

            int size = record.Write(buffer[(idx + Record.Size)..]);
            Record.WriteHeader(record, buffer[idx..], size);

            base.Send(buffer);
        }

        public virtual unsafe void Send(Span<IRecord> records)
        {
            Unsafe.SkipInit(out Container container);
            container.NumRecords = (ushort)records.Length;
            container.Version = ProtocolVersion.Version1;

            uint bufferSize = container.SizeOf() + (uint)(Record.Size * container.NumRecords);

            for (int i = 0; i < container.NumRecords; i++)
            {
                bufferSize += (uint)records[i].SizeOf();
            }

            if (bufferSize > payloadSendBufferLength)
            {
                payloadSendBuffer = (byte*)ReAlloc(payloadSendBuffer, bufferSize);
                payloadSendBufferLength = bufferSize;
            }

            Span<byte> buffer = new(payloadSendBuffer, (int)bufferSize);

            int idx = container.Write(buffer);

            for (int i = 0; i < container.NumRecords; i++)
            {
                var record = records[i];
                int size = record.Write(buffer[(idx + Record.Size)..]);
                idx += Record.WriteHeader(record, buffer[idx..], size) + size;
            }

            base.Send(buffer);
        }

        public virtual unsafe ValueTask<bool> SendAsync(IRecord record, CancellationToken token = default)
        {
            return SendAsync([record], token);
        }

        public virtual unsafe ValueTask<bool> SendAsync(Span<IRecord> records, CancellationToken token = default)
        {
            Memory<byte> buffer = Serialize(records);
            return SendAsync(buffer, SocketFlags.None, token);
        }

        private Memory<byte> Serialize(Span<IRecord> records)
        {
            Unsafe.SkipInit(out Container container);
            container.NumRecords = (ushort)records.Length;
            container.Version = ProtocolVersion.Version1;

            uint bufferSize = container.SizeOf() + (uint)(Record.Size * container.NumRecords);

            for (int i = 0; i < container.NumRecords; i++)
            {
                bufferSize += (uint)records[i].SizeOf();
            }

            if (bufferSize > sendBuffer.Length)
            {
                sendBuffer = new byte[bufferSize];
            }

            Memory<byte> buffer = sendBuffer[..(int)bufferSize];
            var bufferSpan = buffer.Span;

            int idx = container.Write(bufferSpan);

            for (int i = 0; i < container.NumRecords; i++)
            {
                var record = records[i];
                int size = record.Write(bufferSpan[(idx + Record.Size)..]);
                idx += Record.WriteHeader(record, bufferSpan[idx..], size) + size;
            }

            return buffer;
        }

        public virtual unsafe void Send(IList<IRecord> records)
        {
            Unsafe.SkipInit(out Container container);
            container.NumRecords = (ushort)records.Count;
            container.Version = ProtocolVersion.Version1;

            uint bufferSize = container.SizeOf() + (uint)(Record.Size * container.NumRecords);

            for (int i = 0; i < container.NumRecords; i++)
            {
                bufferSize += (uint)records[i].SizeOf();
            }

            if (bufferSize > payloadSendBufferLength)
            {
                payloadSendBuffer = (byte*)ReAlloc(payloadSendBuffer, bufferSize);
                payloadSendBufferLength = bufferSize;
            }

            Span<byte> buffer = new(payloadSendBuffer, (int)bufferSize);

            int idx = container.Write(buffer);

            for (int i = 0; i < container.NumRecords; i++)
            {
                var record = records[i];
                int size = record.Write(buffer[(idx + Record.Size)..]);
                idx += Record.WriteHeader(record, buffer[idx..], size) + size;
            }

            base.Send(buffer);
        }

        public void SendBuffered(Span<IRecord> records)
        {
            queue.AddRange(records);
        }

        public void SendQueue()
        {
            Send(queue);

            for (var i = 0; i < queue.Count; i++)
            {
                queue[i].Free();
            }

            queue.Clear();
        }

        public void SendProtocolError(ErrorCode errorCode, ErrorSeverity severity)
        {
            ProtocolError protocolError = new(errorCode, severity);
            Send(protocolError);
            OnProtocolError(protocolError);
        }

        protected abstract void Process(Record record);

        protected override unsafe void Dispose(bool disposing)
        {
            if (payloadReceiveBuffer != null)
            {
                Free(payloadReceiveBuffer);
                payloadReceiveBuffer = null;
                payloadReceiveBufferLength = 0;
            }
            if (payloadSendBuffer != null)
            {
                Free(payloadSendBuffer);
                payloadSendBuffer = null;
                payloadSendBufferLength = 0;
            }
            base.Dispose(disposing);
        }
    }
}