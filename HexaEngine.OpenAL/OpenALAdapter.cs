namespace HexaEngine.OpenAL
{
    using Hexa.NET.OpenAL;
    using HexaEngine.Core.Audio;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class OpenALAdapter : IAudioAdapter
    {
        public AudioBackend Backend => AudioBackend.OpenAL;

        public int PlatformScore => 0;

        public unsafe IAudioDevice CreateAudioDevice(string? name)
        {
            ALCdevice* device = OpenAL.OpenDevice(name);
            CheckError(device);
            return new OpenALAudioDevice(device);
        }

        public static void Init()
        {
            AudioAdapter.Adapters.Add(new OpenALAdapter());
        }

        public unsafe List<string> GetAvailableDevices()
        {
            List<string> devices = [];
            if (OpenAL.IsExtensionPresent(null, "ALC_ENUMERATION_EXT") != 0)
            {
                var val = OpenAL.GetString(null, OpenAL.ALC_DEVICE_SPECIFIER);

                while (val != null)
                {
                    int len = StrLen(val);
                    devices.Add(Encoding.UTF8.GetString(val, len));
                    val += len + 1;
                    if (*val == '\0')
                    {
                        break;
                    }
                }
            }

            return devices;
        }
    }
}