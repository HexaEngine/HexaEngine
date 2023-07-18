namespace HexaEngine.Lights.Probes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Scenes;

    public interface ILightProbeComponent : IComponent
    {
        public Texture2D? DiffuseTex { get; }

        public Texture2D? SpecularTex { get; }

        public bool IsEnabled { get; }

        public bool IsVaild { get; }

        public ProbeType Type { get; }
    }
}