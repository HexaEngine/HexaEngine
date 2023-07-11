namespace HexaEngine.Lights.Probes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Scenes;

    public interface ILightProbeComponent : IComponent
    {
        public Texture? DiffuseTex { get; }

        public Texture? SpecularTex { get; }

        public bool IsEnabled { get; }

        public bool IsVaild { get; }

        public ProbeType Type { get; }
    }
}