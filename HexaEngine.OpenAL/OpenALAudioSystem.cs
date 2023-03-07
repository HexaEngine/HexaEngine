namespace HexaEngine.OpenAL
{
    using Silk.NET.OpenAL.Extensions.Enumeration;
    using System.Collections.Generic;

    public unsafe class OpenALAudioSystem
    {
        public OpenALAudioSystem()
        {
        }

        public static OpenALAudioDevice CreateAudioDevice(string? name)
        {
            Device* device = alc.OpenDevice(name);
            CheckError(device);
            return new OpenALAudioDevice(device);
        }

        public static List<string> GetAvailableDevices()
        {
            List<string> devices;

            if (alc.TryGetExtension<Enumeration>(null, out var enumeration))
            {
                devices = new(enumeration.GetStringList(GetEnumerationContextStringList.DeviceSpecifiers));
            }
            else
            {
                throw new NotSupportedException();
            }
            return devices;
        }
    }
}