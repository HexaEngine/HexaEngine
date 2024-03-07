namespace HexaEngine.OpenAL
{
    using HexaEngine.Core.Audio;
    using Silk.NET.OpenAL.Extensions.Enumeration;
    using System;
    using System.Collections.Generic;

    public class OpenALAdapter : IAudioAdapter
    {
        public AudioBackend Backend => AudioBackend.OpenAL;

        public int PlatformScore => 0;

        public unsafe IAudioDevice CreateAudioDevice(string? name)
        {
            Device* device = alc.OpenDevice(name);
            CheckError(device);
            return new OpenALAudioDevice(device);
        }

        public static void Init()
        {
            AudioAdapter.Adapters.Add(new OpenALAdapter());
        }

        public unsafe List<string> GetAvailableDevices()
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