namespace HexaEngine.Audio.Common.Flac
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;

    public unsafe struct VorbisComment
    {
        public StdString Vendor;
        public UnsafeList<StdString> UserCommentList;

        public void Read(BitReader br)
        {
            Vendor = br.ReadStdString(Endianness.LittleEndian);
            uint userCommentListLength = br.ReadUInt32LittleEndian();
            UserCommentList.Capacity = userCommentListLength;
            for (uint i = 0; i < userCommentListLength; i++)
            {
                UserCommentList.PushBack(br.ReadStdString(Endianness.LittleEndian));
            }

            for (uint i = 0; (i < userCommentListLength); i++)
            {
                Console.WriteLine(UserCommentList[i].ToString());
            }
        }

        public readonly void Write(BitWriter bw)
        {
            bw.Write(Vendor);
            bw.WriteUInt32LittleEndian(UserCommentList.Size);
            for (uint i = 0; i < UserCommentList.Size; i++)
            {
                bw.WriteStdString(UserCommentList[i], Endianness.LittleEndian);
            }
        }

        public void Release()
        {
            Vendor.Release();
            for (int i = 0; i < UserCommentList.Size; i++)
            {
                UserCommentList[i].Release();
            }
            UserCommentList.Release();
            this = default;
        }
    }
}