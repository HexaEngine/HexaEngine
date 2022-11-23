namespace HexaEngine.Plugins
{
    using HexaEngine.Core;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Plugins.Records;
    using System;
    using System.Buffers.Binary;

    public static class PluginEncoder
    {
        public static unsafe int EncodePlugin(Span<byte> dest, Endianness endianness, Plugin* pPlugin)
        {
            int idx = EncodePluginHeader(dest, endianness, pPlugin->Header);
            for (ulong i = 0; i < pPlugin->Header->RecordCount; i++)
            {
                idx += EncodeRecord(dest[idx..], endianness, pPlugin->Records[i]);
            }

            return idx;
        }

        public static unsafe int EncodePlugin(Span<byte> dest, Plugin* pPlugin)
        {
            Endianness endianness = BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;
            int idx = EncodePluginHeader(dest, endianness, pPlugin->Header);
            for (ulong i = 0; i < pPlugin->Header->RecordCount; i++)
            {
                idx += EncodeRecord(dest[idx..], endianness, pPlugin->Records[i]);
            }

            return idx;
        }

        public static unsafe int EncodePluginHeader(Span<byte> dest, Endianness endianness, PluginHeader* pHeader)
        {
            dest[0] = (byte)endianness;
            pHeader->FormatVersion.Encode(dest[1..], endianness);
            pHeader->Version.Encode(dest[17..], endianness);

            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(dest[33..], pHeader->RecordCount);
            }
            else if (endianness == Endianness.BigEndian)
            {
                BinaryPrimitives.WriteUInt64BigEndian(dest[33..], pHeader->RecordCount);
            }
            else
            {
                throw new InvalidOperationException(nameof(endianness));
            }
            int idx = 41;
            idx += Utilities.WriteString(dest[idx..], endianness, pHeader->Name);
            idx += Utilities.WriteString(dest[idx..], endianness, pHeader->Description);
            return idx;
        }

        public static unsafe int EncodeRecord(Span<byte> dest, Endianness endianness, Record* pRecord)
        {
            switch (pRecord->Header->Type)
            {
                case RecordType.None:
                    break;

                case RecordType.Scene:
                    {
                        SceneRecord* sceneRecord = (SceneRecord*)(void*)pRecord;

                        var len = sceneRecord->Size();
                        pRecord->Header->Length = (uint)len;
                        int idx = EncodeRecordHeader(dest, endianness, pRecord->Header);
                        sceneRecord->Encode(dest[idx..], endianness);
                        return idx + len;
                    }

                case RecordType.Node:
                    break;

                case RecordType.Camera:
                    break;

                case RecordType.Terrain:
                    break;

                case RecordType.Light:
                    break;

                case RecordType.Mesh:
                    break;

                case RecordType.Material:
                    break;
            }
            return 0;
        }

        public static unsafe int EncodeRecordHeader(Span<byte> dest, Endianness endianness, RecordHeader* header)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(dest[0..], header->Id);
                if (header->Parent == null)
                    BinaryPrimitives.WriteUInt64LittleEndian(dest[8..], 0);
                else
                    BinaryPrimitives.WriteUInt64LittleEndian(dest[8..], header->Parent->Header->Id);
                BinaryPrimitives.WriteUInt32LittleEndian(dest[16..], (uint)header->Type);
                BinaryPrimitives.WriteUInt32LittleEndian(dest[20..], header->Length);
                return 24;
            }
            else if (endianness == Endianness.BigEndian)
            {
                BinaryPrimitives.WriteUInt64BigEndian(dest[0..], header->Id);
                if (header->Parent == null)
                    BinaryPrimitives.WriteUInt64BigEndian(dest[8..], 0);
                else
                    BinaryPrimitives.WriteUInt64BigEndian(dest[8..], header->Parent->Header->Id);
                BinaryPrimitives.WriteUInt32BigEndian(dest[16..], (uint)header->Type);
                BinaryPrimitives.WriteUInt32BigEndian(dest[20..], header->Length);
                return 24;
            }
            else
                return 0;
        }

        public static unsafe int ComputePluginSize(Plugin* pPlugin)
        {
            int size = ComputePluginHeaderSize(pPlugin->Header);
            for (ulong i = 0; i < pPlugin->Header->RecordCount; i++)
            {
                size += ComputeRecordSize(pPlugin->Records[i]);
            }
            return size;
        }

        public static unsafe int ComputePluginHeaderSize(PluginHeader* pHeader)
        {
            int idx = 41;
            idx += Utilities.StringSizeNullTerminated(pHeader->Name);
            idx += Utilities.StringSizeNullTerminated(pHeader->Description);
            return idx;
        }

        public static unsafe int ComputeRecordSize(Record* pRecord)
        {
            switch (pRecord->Header->Type)
            {
                case RecordType.None:
                    break;

                case RecordType.Scene:
                    {
                        SceneRecord* sceneRecord = (SceneRecord*)(void*)pRecord;
                        return 24 + sceneRecord->Size();
                    }

                case RecordType.Node:
                    break;

                case RecordType.Camera:
                    break;

                case RecordType.Terrain:
                    break;

                case RecordType.Light:
                    break;

                case RecordType.Mesh:
                    break;

                case RecordType.Material:
                    break;
            }
            return 0;
        }
    }
}