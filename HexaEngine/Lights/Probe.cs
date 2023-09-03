namespace HexaEngine.Lights
{
    using HexaEngine.Core.Scenes;
    using System.Numerics;

    public abstract class Probe : GameObject
    {
        public abstract ProbeType ProbeType { get; }

        public float Exposure { get; set; }

        public float HorizonCutOff { get; set; }

        public Vector3 Orientation { get; set; }

        public float Range { get; set; }
    }
}