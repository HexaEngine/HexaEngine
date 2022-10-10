namespace HexaEngine.Plugins
{
    using HexaEngine.Core.Unsafes;
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    public unsafe class PluginReader
    {
        private readonly Stream baseStream;

        public PluginReader(Stream baseStream)
        {
            this.baseStream = baseStream;
        }

        public Stream BaseStream => baseStream;

        public Plugin Read()
        {
            Span<byte> data = new byte[baseStream.Length];
            baseStream.Read(data);
            PluginHeader header = default;
            int idx = 0;
            Endianness endianness = header.Endianness = (Endianness)data[idx++];
            idx += UnsafeString.Read(&header.Name, endianness, data[idx..]);
            idx += UnsafeString.Read(&header.Description, endianness, data[idx..]);

            int sizeVersionStruct = sizeof(Version);
            fixed (byte* ptr = data[idx..])
                header.Version = *(Version*)ptr;
            idx += sizeVersionStruct;

            fixed (byte* ptr = data[idx..])
                header.FormatVersion = *(Version*)ptr;
            idx += sizeVersionStruct;

            Plugin plugin = new() { Header = header };

            if (header.FormatVersion.Major == 1)
                Read1000(data[idx..], &plugin);

            return plugin;
        }

        private void Read1000(Span<byte> data, Plugin* plugin)
        {
            Endianness endianness = plugin->Header.Endianness;
            int idx = 0;
            if (endianness == Endianness.LittleEndian)
                plugin->RecordCount = BinaryPrimitives.ReadInt32LittleEndian(data);
            if (endianness == Endianness.BigEndian)
                plugin->RecordCount = BinaryPrimitives.ReadInt32BigEndian(data);
            idx += 4;
            fixed (Record* records = new Record[plugin->RecordCount])
            {
                plugin->Records = records;
            }

            int headerSize = sizeof(RecordHeader);
            for (int i = 0; i < plugin->RecordCount; i++)
            {
                Record record = default;
                record.Header.Decode(data.Slice(idx, headerSize), endianness);
                idx += headerSize;
                switch (record.Header.Type)
                {
                    case RecordType.Scene:
                        {
                            SceneRecord scene = default;
                            scene.Decode(data.Slice(idx, record.Header.Length), endianness);
                            record.Data = &scene;
                        }
                        break;

                    case RecordType.Node:
                        break;

                    case RecordType.Camera:
                        break;

                    case RecordType.Light:
                        break;

                    case RecordType.Mesh:
                        break;

                    case RecordType.Material:
                        break;
                }
                plugin->Records[i] = record;
                idx += record.Header.Length;
            }
        }

        public void Dispose()
        {
            baseStream?.Dispose();
        }
    }
}