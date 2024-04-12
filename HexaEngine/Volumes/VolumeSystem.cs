namespace HexaEngine.Volumes
{
    using HexaEngine.Scenes;

    public class VolumeSystem : ISceneSystem
    {
        public string Name { get; } = "VolumeSystem";

        public SystemFlags Flags { get; }
    }
}