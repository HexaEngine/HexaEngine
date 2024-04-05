namespace HexaEngine.Lights
{
    using HexaEngine.Components;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using Newtonsoft.Json;
    using System.Numerics;

    public abstract class LightSource : GameObject
    {
        internal bool InUpdateQueue;
        internal uint QueueIndex;
        protected Vector4 color = Vector4.One;
        private float intensity = 1;

        public abstract LightType LightType { get; }

        #region General

        [EditorCategory("General")]
        [EditorProperty("Color", EditorPropertyMode.Colorpicker)]
        public Vector4 Color { get => color; set => SetAndNotifyWithEqualsTest(ref color, value); }

        [EditorCategory("General")]
        [EditorProperty("Intensity")]
        public float Intensity { get => intensity; set => SetAndNotifyWithEqualsTest(ref intensity, value); }

        #endregion General
    }

    public abstract class Light : LightSource
    {
        private float range = 50;
        private bool shadowMapEnable;
        private ShadowResolution shadowMapResolution;
        private ShadowUpdateMode shadowMapUpdateMode = ShadowUpdateMode.OnDemand;
        private float shadowMapSlopeScaleDepthBias;
        private float shadowMapNormalBias;
        private float shadowMapSoftness;
        private bool contactShadowsEnable;
        private float contactShadowsThickness;
        private uint contactShadowsMaxSteps;
        private float contactShadowsMaxRayDistance;
        private float contactShadowsMaxDepthDistance;
        private bool volumetricsEnable;
        private float volumetricsMultiplier;

        public Light()
        {
            AddComponentSingleton<SphereSelectionComponent>();
        }

        [JsonConstructor]
        public Light(Vector4 color)
        {
            this.color = color;
        }

        #region General

        [EditorCategory("General")]
        [EditorProperty("Range")]
        public float Range { get => range; set => SetAndNotifyWithEqualsTest(ref range, value); }

        #endregion General

        #region Shadows

        #region Shadow Map

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Enable")]
        public bool ShadowMapEnable { get => shadowMapEnable; set => SetAndNotifyWithEqualsTest(ref shadowMapEnable, value); }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty<ShadowUpdateMode>("Update Mode")]
        public ShadowUpdateMode ShadowMapUpdateMode { get => shadowMapUpdateMode; set => shadowMapUpdateMode = value; }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty<ShadowResolution>("Resolution")]
        public ShadowResolution ShadowMapResolution
        {
            get => shadowMapResolution; set
            {
                SetAndNotify(ref shadowMapResolution, value);
                DestroyShadowMap();
            }
        }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Slope-Scale depth Bias")]
        public float ShadowMapSlopeScaleDepthBias { get => shadowMapSlopeScaleDepthBias; set => shadowMapSlopeScaleDepthBias = value; }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Normal Bias")]
        public float ShadowMapNormalBias { get => shadowMapNormalBias; set => shadowMapNormalBias = value; }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Softness")]
        public float ShadowMapSoftness { get => shadowMapSoftness; set => shadowMapSoftness = value; }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Light Bleeding Reduction")]
        public float ShadowMapLightBleedingReduction { get => shadowMapSoftness; set => shadowMapSoftness = value; }

        [JsonIgnore]
        public abstract int ShadowMapSize { get; }

        #endregion Shadow Map

        #region Contact Shadows

        [EditorCategory("Contact Shadows", "Shadows")]
        [EditorProperty("Enable")]
        public bool ContactShadowsEnable { get => contactShadowsEnable; set => contactShadowsEnable = value; }

        [EditorCategory("Contact Shadows", "Shadows")]
        [EditorProperty("Thickness")]
        public float ContactShadowsThickness { get => contactShadowsThickness; set => contactShadowsThickness = value; }

        [EditorCategory("Contact Shadows", "Shadows")]
        [EditorProperty("Max Steps")]
        public uint ContactShadowsMaxSteps { get => contactShadowsMaxSteps; set => contactShadowsMaxSteps = value; }

        [EditorCategory("Contact Shadows", "Shadows")]
        [EditorProperty("Max Ray Distance")]
        public float ContactShadowsMaxRayDistance { get => contactShadowsMaxRayDistance; set => contactShadowsMaxRayDistance = value; }

        [EditorCategory("Contact Shadows", "Shadows")]
        [EditorProperty("Max depth Distance")]
        public float ContactShadowsMaxDepthDistance { get => contactShadowsMaxDepthDistance; set => contactShadowsMaxDepthDistance = value; }

        #endregion Contact Shadows

        #endregion Shadows

        #region Volumetrics

        [EditorCategory("Volumetrics")]
        [EditorProperty("Enable")]
        public bool VolumetricsEnable { get => volumetricsEnable; set => volumetricsEnable = value; }

        [EditorCategory("Volumetrics")]
        [EditorProperty("Multiplier")]
        public float VolumetricsMultiplier { get => volumetricsMultiplier; set => volumetricsMultiplier = value; }

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

        public abstract bool UpdateShadowMapSize(Camera camera, ShadowAtlas atlas);

        public abstract void CreateShadowMap(IGraphicsDevice device, ShadowAtlas atlas);

        public abstract void DestroyShadowMap();

        public uint GetQueueIndex() => QueueIndex;
    }
}