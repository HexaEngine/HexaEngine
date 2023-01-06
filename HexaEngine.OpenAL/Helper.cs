namespace HexaEngine.OpenAL
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static class Helper
    {
        public static readonly AL al = AL.GetApi(true);
        public static readonly ALContext alc = ALContext.GetApi(true);

        public static unsafe bool CheckError(Device* device, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        {
            ContextError error = alc.GetError(device);
            if (error != ContextError.NoError)
            {
                Debug.WriteLine($"***OpenAL ERROR*** ({filename}: {line}, {name})");
                throw error switch
                {
                    ContextError.InvalidValue => new OpenALException("ALC_INVALID_VALUE: an invalid value was passed to an OpenAL function"),
                    ContextError.InvalidDevice => new OpenALException("ALC_INVALID_DEVICE: a bad device was passed to an OpenAL function"),
                    ContextError.InvalidContext => new OpenALException("ALC_INVALID_CONTEXT: a bad context was passed to an OpenAL function"),
                    ContextError.InvalidEnum => new OpenALException("ALC_INVALID_ENUM: an unknown enum value was passed to an OpenAL function"),
                    ContextError.OutOfMemory => new OpenALException("ALC_OUT_OF_MEMORY: an unknown enum value was passed to an OpenAL function"),
                    _ => new OpenALException($"UNKNOWN ALC ERROR: {error}"),
                };
            }
            return true;
        }
    }
}