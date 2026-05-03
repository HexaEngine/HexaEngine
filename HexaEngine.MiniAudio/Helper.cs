namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;
    using HexaEngine.Core.Audio;

    public static class Helper
    {
        public static MaFormat Convert(AudioFormat format)
        {
            return format switch
            {
                AudioFormat.U8 => MaFormat.U8,
                AudioFormat.S16 => MaFormat.S16,
                AudioFormat.S24 => MaFormat.S24,
                AudioFormat.S32 => MaFormat.S32,
                AudioFormat.F32 => MaFormat.F32,
                _ => throw new NotSupportedException()
            };
        }

        public static MaPanMode Convert(PanMode mode)
        {
            return mode switch
            {
                PanMode.Balance => MaPanMode.Balance,
                PanMode.Pan => MaPanMode.Pan,
                _ => throw new NotSupportedException()
            };
        }

        public static PanMode ConvertBack(MaPanMode mode)
        {
            return mode switch
            {
                MaPanMode.Balance => PanMode.Balance,
                MaPanMode.Pan => PanMode.Pan,
                _ => throw new NotSupportedException()
            };
        }

        public static MaAttenuationModel Convert(AttenuationModel model)
        {
            return model switch
            {
                AttenuationModel.None => MaAttenuationModel.None,
                AttenuationModel.Inverse => MaAttenuationModel.Inverse,
                AttenuationModel.Linear => MaAttenuationModel.Linear,
                AttenuationModel.Exponential => MaAttenuationModel.Exponential,
                _ => throw new NotSupportedException()
            };
        }

        public static AttenuationModel ConvertBack(MaAttenuationModel model)
        {
            return model switch
            {
                MaAttenuationModel.None => AttenuationModel.None,
                MaAttenuationModel.Inverse => AttenuationModel.Inverse,
                MaAttenuationModel.Linear => AttenuationModel.Linear,
                MaAttenuationModel.Exponential => AttenuationModel.Exponential,
                _ => throw new NotSupportedException()
            };
        }

        public static MaSoundFlags Convert(SoundFlags flags)
        {
            MaSoundFlags maflags = 0;
            if ((flags & SoundFlags.Streaming) != 0)
            {
                maflags |= MaSoundFlags.FlagStream;
            }
            if ((flags & SoundFlags.Decode) != 0)
            {
                maflags |= MaSoundFlags.FlagDecode;
            }
            if ((flags & SoundFlags.UnknownLength) != 0)
            {
                maflags |= MaSoundFlags.FlagUnknownLength;
            }
            return maflags;
        }
    }
}