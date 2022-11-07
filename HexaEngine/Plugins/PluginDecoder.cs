namespace HexaEngine.Plugins
{
    using HexaEngine.Core;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Plugins.Records;
    using System;
    using System.Buffers.Binary;

    public static class PluginDecoder
    {
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
            idx += pHeader->Name->Sizeof();
            idx += pHeader->Description->Sizeof();
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

        public static unsafe int DecodePlugin(Span<byte> src, Plugin** ppPlugin)
        {
            Plugin* pPlugin = Utilities.Alloc<Plugin>();
            *ppPlugin = pPlugin;
            Endianness endianness = (Endianness)src[0];
            int idx = DecodePluginHeader(src, endianness, &pPlugin->Header);
            fixed (Record* pRecords = new Record[pPlugin->Header->RecordCount])
            {
                pPlugin->Records = &pRecords;
            }
            for (ulong i = 0; i < pPlugin->Header->RecordCount; i++)
            {
                idx += DecodeRecord(src[idx..], endianness, &pPlugin->Records[i]);
            }

            return idx;
        }

        public static unsafe int DecodePluginHeader(Span<byte> src, Endianness endianness, PluginHeader** ppHeader)
        {
            PluginHeader* pHeader = Utilities.Alloc<PluginHeader>();
            *ppHeader = pHeader;

            pHeader->Endianness = endianness;
            pHeader->FormatVersion.Decode(src[1..], endianness);
            pHeader->Version.Decode(src[17..], endianness);

            if (endianness == Endianness.LittleEndian)
            {
                pHeader->RecordCount = BinaryPrimitives.ReadUInt64LittleEndian(src[33..]);
            }
            else if (endianness == Endianness.BigEndian)
            {
                pHeader->RecordCount = BinaryPrimitives.ReadUInt64BigEndian(src[33..]);
            }
            else
            {
                throw new InvalidOperationException(nameof(endianness));
            }
            int idx = 41;

            idx += UnsafeString.Read(&pHeader->Name, endianness, src[idx..]);
            idx += UnsafeString.Read(&pHeader->Description, endianness, src[idx..]);
            return idx;
        }

        public static unsafe int DecodeRecord(Span<byte> src, Endianness endianness, Record** ppRecord)
        {
            RecordHeader* pHeader;
            int idx = DecodeRecordHeader(src, endianness, &pHeader);

            switch (pHeader->Type)
            {
                case RecordType.None:
                    break;

                case RecordType.Scene:
                    {
                        SceneRecord* pSceneRecord = Utilities.Alloc<SceneRecord>();
                        pSceneRecord->Header = pHeader;
                        *ppRecord = (Record*)pSceneRecord;
                        return idx + pSceneRecord->Decode(src[idx..], endianness);
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

        public static unsafe int DecodeRecordHeader(Span<byte> src, Endianness endianness, RecordHeader** ppHeader)
        {
            RecordHeader* pHeader = Utilities.Alloc<RecordHeader>();
            *ppHeader = pHeader;
            if (endianness == Endianness.LittleEndian)
            {
                pHeader->Id = BinaryPrimitives.ReadUInt64LittleEndian(src[0..]);
                pHeader->IdParent = BinaryPrimitives.ReadUInt64LittleEndian(src[8..]);
                pHeader->Type = (RecordType)BinaryPrimitives.ReadUInt32LittleEndian(src[16..]);
                pHeader->Length = BinaryPrimitives.ReadUInt32LittleEndian(src[20..]);
                return 24;
            }
            else if (endianness == Endianness.BigEndian)
            {
                pHeader->Id = BinaryPrimitives.ReadUInt64BigEndian(src[0..]);
                pHeader->IdParent = BinaryPrimitives.ReadUInt64BigEndian(src[8..]);
                pHeader->Type = (RecordType)BinaryPrimitives.ReadUInt32BigEndian(src[16..]);
                pHeader->Length = BinaryPrimitives.ReadUInt32BigEndian(src[20..]);
                return 24;
            }
            else
                return 0;
        }
    }
}