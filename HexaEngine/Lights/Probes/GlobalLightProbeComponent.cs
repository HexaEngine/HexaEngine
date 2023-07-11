namespace HexaEngine.Lights.Probes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using Newtonsoft.Json;
    using System.Numerics;

    public class GlobalLightProbeComponent : ILightProbeComponent
    {
        protected GameObject gameObject;
        protected Texture? diffuseTex;
        protected Texture? specularTex;
        protected bool isEnabled;
        protected bool isVaild;

        [JsonIgnore]
        public Texture? DiffuseTex => diffuseTex;

        [JsonIgnore]
        public Texture? SpecularTex => specularTex;

        [JsonIgnore]
        public bool IsEnabled => gameObject?.IsEnabled ?? false;

        [JsonIgnore]
        public bool IsVaild => isVaild;

        [JsonIgnore]
        public ProbeType Type => ProbeType.Global;

        [EditorProperty("Exposure")]
        public float Exposure { get; set; }

        [EditorProperty("Horizon cut off")]
        public float HorizonCutOff { get; set; }

        [EditorProperty("Orientation")]
        public Vector3 Orientation { get; set; }

        public virtual void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        public virtual void Destory()
        {
        }
    }
}