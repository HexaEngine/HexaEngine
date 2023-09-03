namespace HexaEngine.Lights.Probes
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Lights;

    [EditorGameObject<StaticLightProbe>("Static Light Probe")]
    public class StaticLightProbe : Probe
    {
        public override ProbeType ProbeType => ProbeType.Light;

        [EditorButton("Bake Probe")]
        public void BakeProbe()
        {
        }
    }
}