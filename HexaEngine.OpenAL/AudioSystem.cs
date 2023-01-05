namespace HexaEngine.OpenAL
{
    using Silk.NET.OpenAL.Extensions.Enumeration;
    using System.Collections.Generic;

    public unsafe class AudioSystem
    {
        public AudioSystem()
        {
        }

        public static AudioDevice CreateAudioDevice(string? name)
        {
            Device* device = alc.OpenDevice(name);
            CheckError(device);
            return new AudioDevice(device);
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