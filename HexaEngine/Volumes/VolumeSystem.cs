namespace HexaEngine.Volumes
{
    using HexaEngine.Scenes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class VolumeSystem : ISceneSystem
    {
        public string Name { get; }

        public SystemFlags Flags { get; }
    }
}