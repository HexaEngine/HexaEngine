namespace HexaEngine.Lights.Probes
{
    using System.Numerics;

    public struct GlobalProbeData
    {
        public float Exposure;
        public float HorizonCutOff;
        public Vector3 Orientation;

        public static implicit operator GlobalProbeData(GlobalLightProbeComponent globalLightProbe)
        {
            return new GlobalProbeData()
            {
                Exposure = globalLightProbe.Exposure,
                HorizonCutOff = globalLightProbe.HorizonCutOff,
                Orientation = globalLightProbe.Orientation,
            };
        }
    }
}