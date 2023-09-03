namespace HexaEngine.Lights.Structs
{
    using System.Numerics;
    using HexaEngine.Lights;

    public struct ProbeData
    {
        public float Exposure;
        public float HorizonCutOff;
        public Vector3 Orientation;

        public static implicit operator ProbeData(Probe probe)
        {
            return new ProbeData()
            {
                Exposure = probe.Exposure,
                HorizonCutOff = probe.HorizonCutOff,
                Orientation = probe.Orientation,
            };
        }
    }
}