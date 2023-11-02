namespace HexaEngine.Audio.Common.Flac
{
    public unsafe struct MetadataBlock
    {
        public MetadataBlockHeader BlockHeader;
        public void* BlockData;

        public void Read(BitReader br)
        {
            BlockHeader.Read(br);
            switch (BlockHeader.Type)
            {
                case BlockType.StreamInfo:
                    StreamInfo* si = AllocT<StreamInfo>();
                    si->Read(br);
                    BlockData = si;
                    break;

                case BlockType.Padding:
                    Padding.Read(br, BlockHeader);
                    BlockData = null;
                    break;

                case BlockType.Application:
                    Application* app = AllocT<Application>();
                    app->Read(br, BlockHeader);
                    BlockData = app;
                    break;

                case BlockType.SeekTable:
                    SeekTable* st = AllocT<SeekTable>();
                    st->Read(br, BlockHeader);
                    BlockData = st;
                    break;

                case BlockType.VorbisComment:
                    VorbisComment* vc = AllocT<VorbisComment>();
                    vc->Read(br);
                    BlockData = vc;
                    break;

                case BlockType.CueSheet:
                    CueSheet* cs = AllocT<CueSheet>();
                    cs->Read(br);
                    BlockData = cs;
                    break;

                case BlockType.Picture:
                    Picture* p = AllocT<Picture>();
                    p->Read(br);
                    BlockData = p;
                    break;

                case BlockType.Reserved:
                    break;

                case BlockType.Invalid:
                    break;

                default:
                    br.BytePosition += BlockHeader.Length;
                    break;
            }
        }

        public void Write(BitWriter bw)
        {
            long headerPos = bw.BytePosition;
            bw.BytePosition += 4;

            switch (BlockHeader.Type)
            {
                case BlockType.StreamInfo:
                    ((StreamInfo*)BlockData)->Write(bw);
                    break;

                case BlockType.Padding:
                    Padding.Write(bw, BlockHeader);
                    break;

                case BlockType.Application:
                    ((Application*)BlockData)->Write(bw, BlockHeader);
                    break;

                case BlockType.SeekTable:
                    ((SeekTable*)BlockData)->Write(bw);
                    break;

                case BlockType.VorbisComment:
                    ((SeekTable*)BlockData)->Write(bw);
                    break;

                case BlockType.CueSheet:
                    ((CueSheet*)BlockData)->Write(bw);
                    break;

                case BlockType.Picture:
                    ((Picture*)BlockData)->Write(bw);
                    break;

                case BlockType.Reserved:
                    break;

                case BlockType.Invalid:
                    break;

                default:
                    bw.BytePosition += BlockHeader.Length;
                    break;
            }

            long dataEnd = bw.BytePosition;
            bw.BytePosition = headerPos;
            BlockHeader.Length = (uint)(dataEnd - (headerPos + 4));
            BlockHeader.Write(bw);
            bw.BytePosition = dataEnd;
        }

        public void Release()
        {
            switch (BlockHeader.Type)
            {
                case BlockType.Application:
                    ((Application*)BlockData)->Release();
                    break;

                case BlockType.SeekTable:
                    ((SeekTable*)BlockData)->Release();
                    break;

                case BlockType.VorbisComment:
                    ((VorbisComment*)BlockData)->Release();
                    break;

                case BlockType.CueSheet:
                    ((CueSheet*)BlockData)->Release();
                    break;

                case BlockType.Picture:
                    ((Picture*)BlockData)->Release();
                    break;
            }

            if (BlockData != null)
            {
                Free(BlockData);
            }
            BlockData = null;

            this = default;
        }
    }
}