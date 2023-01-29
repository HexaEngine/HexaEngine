namespace HexaEngine.Core.Animations
{
    using HexaEngine.Core.IO;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class Animation
    {
        public readonly string Name;
        public readonly double Duration;
        public readonly double TicksPerSecond;
        public readonly List<NodeChannel> NodeChannels = new();
        public readonly List<MeshChannel> MeshChannels = new();
        public readonly List<MorphMeshChannel> MorphMeshChannels = new();

        public Animation(string name, double duration, double ticksPerSecond)
        {
            Name = name;
            Duration = duration;
            TicksPerSecond = ticksPerSecond;
        }

        [JsonConstructor]
        public Animation(string name, double duration, double ticksPerSecond, IEnumerable<NodeChannel> nodeChannels, IEnumerable<MeshChannel> meshChannels, IEnumerable<MorphMeshChannel> morphMeshChannels)
        {
            Name = name;
            Duration = duration;
            TicksPerSecond = ticksPerSecond;
            NodeChannels.AddRange(nodeChannels);
            MeshChannels.AddRange(meshChannels);
            MorphMeshChannels.AddRange(morphMeshChannels);
        }

        public void Save(string dir)
        {
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, Name + ".anim"), JsonConvert.SerializeObject(this));
        }

        public static Animation Load(string path)
        {
            return JsonConvert.DeserializeObject<Animation>(FileSystem.ReadAllText(Path.Combine(Paths.CurrentAnimationsPath, path + ".anim")));
        }

        public static Animation LoadExternal(string path)
        {
            return JsonConvert.DeserializeObject<Animation>(File.ReadAllText(path));
        }
    }
}