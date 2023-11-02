namespace HexaEngine.Audio.Common.Flac
{
    public enum ChannelAssignment : byte
    {
        Mono = 0b0000,
        StereoLeftRight = 0b0001,
        StereoLeftRightCenter = 0b0010,
        QuadFrontLeftRightBackLeftRight = 0b0011,
        Surround5FrontLeftRightCenterBackLeftRight = 0b0100,
        Surround6FrontLeftRightCenterLFESurroundLeftRight = 0b0101,
        Surround7FrontLeftRightCenterLFESurroundBackCenterSideLeftRight = 0b0110,
        Surround8FrontLeftRightCenterLFEBRSLRSR = 0b0111,
        LeftSideStereo = 0b1000,
        RightSideStereo = 0b1001,
        MidSideStereo = 0b1010,
        Reserved = 0b1011 // 0b1011 to 0b1111 are reserved
    }
}