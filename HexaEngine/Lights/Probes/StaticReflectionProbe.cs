namespace HexaEngine.Lights.Probes
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Lights;

    [EditorGameObject<StaticReflectionProbe>("Static Reflection Probe")]
    public class StaticReflectionProbe : Probe
    {
        public override ProbeType ProbeType => ProbeType.Reflection;

        [EditorButton("Bake Probe")]
        public void BakeProbe()
        {
        }
    }
}