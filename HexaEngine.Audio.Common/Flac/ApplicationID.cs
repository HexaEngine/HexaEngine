namespace HexaEngine.Audio.Common.Flac
{
    public enum ApplicationID : uint
    {
        FlacFile = 0x41544348, // "ATCH"
        BeSolo = 0x42534F4C, // "BSOL"
        BugsPlayer = 0x42554753, // "BUGS"
        GoldWaveCuePoints = 0x43756573, // "Cues"
        CUESplitter = 0x46696361, // "Fica"
        FLACTools = 0x46746F6C, // "Ftol"
        MOTBMetaCzar = 0x4D4F5442, // "MOTB"
        MP3StreamEditor = 0x4D505345, // "MPSE"
        MusicML = 0x4D754D4C, // "MuML"
        SoundDevicesRIFF = 0x52494646, // "RIFF"
        SoundFontFLAC = 0x5346464C, // "SFFL"
        SonyCreativeSoftware = 0x534F4E59, // "SONY"
        FLACsqueeze = 0x5351455A, // "SQEZ"
        TwistedWave = 0x54745776, // "TtWv"
        UITSEmbeddingTools = 0x55495453, // "UITS"
        FLACAIFFChunkStorage = 0x61696666, // "aiff"
        FlacImage = 0x696D6167, // "imag"
        ParseableEmbeddedExtensibleMetadata = 0x7065656D, // "peem"
        QFLACStudio = 0x71667374, // "qfst"
        FLACRIFFChunkStorage = 0x72696666, // "riff"
        TagTuner = 0x74756E65, // "tune"
        FLACWave64ChunkStorage = 0x773634C0, // "w64 "
        XBAT = 0x78626174, // "xbat"
        XMCD = 0x786D6364 // "xmcd"
    }
}