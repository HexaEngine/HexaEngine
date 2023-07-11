namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;
    using System.Numerics;

    public abstract class Light : GameObject
    {
        internal bool InUpdateQueue;
        internal uint QueueIndex;
        protected const float DegToRadFactor = 0.0174532925f;
        protected Vector4 color = Vector4.One;
        private float range = 50;
        private float intensity = 1;
        private bool shadowMapEnable;

        public abstract LightType LightType { get; }

        #region General

        [EditorCategory("General")]
        [EditorProperty("Color", EditorPropertyMode.Colorpicker)]
        public Vector4 Color { get => color; set => SetAndNotifyWithEqualsTest(ref color, value); }

        [EditorCategory("General")]
        [EditorProperty("Range")]
        public float Range { get => range; set => SetAndNotifyWithEqualsTest(ref range, value); }

        [EditorCategory("General")]
        [EditorProperty("Intensity")]
        public float Intensity { get => intensity; set => SetAndNotifyWithEqualsTest(ref intensity, value); }

        #endregion General

        #region Shadows

        #region Shadow Map

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Enable")]
        public bool ShadowMapEnable { get => shadowMapEnable; set => SetAndNotifyWithEqualsTest(ref shadowMapEnable, value); }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty<ShadowUpdateMode>("Update Mode")]
        public ShadowUpdateMode ShadowMapUpdateMode { get; set; }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty<ShadowResolution>("Resolution")]
        public ShadowResolution ShadowMapResolution { get; set; }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Slope-Scale Depth Bias")]
        public float ShadowMapSlopeScaleDepthBias { get; set; }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Normal Bias")]
        public float ShadowMapNormalBias { get; set; }

        #endregion Shadow Map

        #region Contact Shadows

        [EditorCategory("Contact Shadows", "Shadows")]
        [EditorProperty("Enable")]
        public bool ContactShadowsEnable { get; set; }

        [EditorCategory("Contact Shadows", "Shadows")]
        [EditorProperty("Thickness")]
        public float ContactShadowsThickness { get; set; }

        [EditorCategory("Contact Shadows", "Shadows")]
        [EditorProperty("Max Steps")]
        public uint ContactShadowsMaxSteps { get; set; }

        [EditorCategory("Contact Shadows", "Shadows")]
        [EditorProperty("Max Ray Distance")]
        public float ContactShadowsMaxRayDistance { get; set; }

        [EditorCategory("Contact Shadows", "Shadows")]
        [EditorProperty("Max Depth Distance")]
        public float ContactShadowsMaxDepthDistance { get; set; }

        #endregion Contact Shadows

        #endregion Shadows

        #region Volumetrics

        [EditorCategory("Volumetrics")]
        [EditorProperty("Enable")]
        public bool VolumetricsEnable { get; set; }

        [EditorCategory("Volumetrics")]
        [EditorProperty("Multiplier")]
        public float VolumetricsMultiplier { get; set; }

        #endregion Volumetrics

        public abstract bool HasShadowMap { get; }

        [JsonIgnore]
        public Vector3 Position => Transform.GlobalPosition;

        [JsonIgnore]
        public Vector3 Direction => Transform.Forward;

        /// <summary>
        /// Tests if an object that moved affects the shadow volume
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public abstract bool IntersectFrustum(BoundingBox box);

        public abstract IShaderResourceView? GetShadowMap();

        public abstract void CreateShadowMap(IGraphicsDevice device, ShadowAtlas atlas);

        public abstract void DestroyShadowMap(ShadowAtlas atlas);

        public uint GetQueueIndex() => QueueIndex;
    }
}