namespace TestApp
{
    using HexaEngine.Mathematics;
    using HexaEngine.OpenAL;
    using Silk.NET.Core.Native;
    using Silk.NET.OpenAL;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Numerics;

    public class Program
    {
        private static List<Tes> e = new List<Tes>();

        public unsafe delegate void LPALCRESETDEVICESOFT(Device* device, int* attributes);

        public const int AL_SOURCE_SPATIALIZE_SOFT = 0x1214;
        public const int ALC_HRTF_SOFT = 0x1992;
        public const int ALC_TRUE = 1;

        public static unsafe void Main()
        {
            var devs = AudioSystem.GetAvailableDevices();
            AudioDevice device = AudioSystem.CreateAudioDevice(null);

            /*if (alc.IsExtensionPresent("ALC_SOFT_HRTF"))
            {
                delegate*<Device*, int*, int> alcResetDeviceSOFT = (delegate*<Device*, int*, int>)al.GetProcAddress("alcResetDeviceSOFT");

                int* attributes = Alloc<int>(3);
                attributes[0] = ALC_HRTF_SOFT;
                attributes[1] = ALC_TRUE;
                attributes[2] = 0;

                var result = alcResetDeviceSOFT(device.Device, attributes);

                int hrtfenabled = 0;
                delegate*<Device*, int, int, int*, void> alcGetIntegerv = (delegate*<Device*, int, int, int*, void>)al.GetProcAddress("alcGetIntegerv");
                alcGetIntegerv(device.Device, ALC_HRTF_SOFT, 1, &hrtfenabled);

                Debug.WriteLine($"HRTF is {(hrtfenabled == 1 ? "enabled" : "not enabled")}");
            }*/

            var stream = device.CreateAudioStream(File.OpenRead("piano2.wav"));
            var source = al.GenSource();
            stream.Initialize(source);
            stream.Looping = true;
            al.SourcePlay(source);
            al.SetSourceProperty(source, (SourceBoolean)AL_SOURCE_SPATIALIZE_SOFT, true);
            float x = -100;

            al.DistanceModel(DistanceModel.ExponentDistance);
            al.SetListenerProperty(ListenerVector3.Position, new Vector3(0, 0, 0));
            al.SetListenerProperty(ListenerVector3.Velocity, new Vector3(0, 0, 0));
            fixed (Vector3* d = new Vector3[2] { new(0, 0, -1), new(0, 1, 0) })
                al.SetListenerProperty(ListenerFloatArray.Orientation, (float*)d);

            al.SetSourceProperty(source, SourceFloat.Pitch, 1);
            al.SetSourceProperty(source, SourceFloat.Gain, 1);
            al.SetSourceProperty(source, SourceFloat.RolloffFactor, 0.5f);
            al.SetSourceProperty(source, SourceVector3.Position, new Vector3(0, -10, 10));
            al.SetSourceProperty(source, SourceVector3.Velocity, new Vector3(0, 0, 0));

            while (true)
            {
                Thread.Sleep(10);

                stream.Update(source);
            }
        }

        private static async void Do(int i)
        {
            Tes tes = new();
            e.Add(tes);
            for (int j = 0; j < 16; j++)
            {
                await tes.CreateInstance(i + ":" + j);
            }
        }

        private class Tes
        {
            private readonly ConcurrentDictionary<string, Archtype> t = new();
            private List<Archtype> s = new();
            private SemaphoreSlim semaphore = new(1);
            private int counter;

            public async Task<Archtype> CreateArchtype(string name)
            {
                await semaphore.WaitAsync();

                if (!t.TryGetValue(name, out var type))
                {
                    type = new(Interlocked.Increment(ref counter));
                    t.TryAdd(name, type);
                    s.Add(type);
                }

                semaphore.Release();
                return type;
            }

            public async Task<Instance> CreateInstance(string name)
            {
                await semaphore.WaitAsync();

                if (!t.TryGetValue(name, out var type))
                {
                    type = new(Interlocked.Increment(ref counter));
                    t.TryAdd(name, type);
                    s.Add(type);
                }

                var instance = type.CreateInstance();

                semaphore.Release();
                return instance;
            }
        }

        private class Archtype
        {
            private List<Instance> s = new();
            private int id;
            private int counter;

            public Archtype(int id)
            {
                this.id = id;
            }

            public Instance CreateInstance()
            {
                Instance instance = new(this, Interlocked.Increment(ref counter));
                s.Add(instance);
                return instance;
            }

            public override string ToString()
            {
                return id.ToString();
            }
        }

        private class Instance
        {
            private readonly Archtype archtype;
            private int id;

            public Instance(Archtype archtype, int id)
            {
                this.archtype = archtype;
                this.id = id;
            }

            public override string ToString()
            {
                return $"{archtype}:{id}";
            }
        }
    }
}