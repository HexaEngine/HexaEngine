namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.ImGuiNET;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using Newtonsoft.Json;
    using System;
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
        private ShadowResolution shadowMapResolution;

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
        public ShadowUpdateMode ShadowMapUpdateMode { get; set; } = ShadowUpdateMode.OnDemand;

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty<ShadowResolution>("Resolution")]
        public ShadowResolution ShadowMapResolution { get => shadowMapResolution; set => SetAndNotify(ref shadowMapResolution, value); }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Slope-Scale depth Bias")]
        public float ShadowMapSlopeScaleDepthBias { get; set; }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("normal Bias")]
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
        [EditorProperty("Max depth Distance")]
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

        public abstract void DestroyShadowMap();

        public uint GetQueueIndex() => QueueIndex;

        public void ComputeImportance(Camera camera, ShadowAtlas atlas)
        {
            var camPos = camera.Transform.GlobalPosition;
            var lightPos = Transform.GlobalPosition;
            var distance = Vector3.Distance(camPos, lightPos);
            var lightRange = range;
            var profile = shadowMapResolution;

            var maxResolution = profile switch
            {
                ShadowResolution.Ultra => 1024,
                ShadowResolution.High => 512,
                ShadowResolution.Medium => 256,
                ShadowResolution.Low => 128,
                _ => 1024,// Default to the highest resolution
            };

            var distanceImportance = 1.0f - MathUtil.Clamp01(distance / lightRange);
            var scaledImportance = atlas.LayerCount - 1 - MathUtil.Clamp(distanceImportance * atlas.LayerCount, 0f, atlas.LayerCount - 1);
            var maxResolutionIndex = atlas.SizeToIndex(maxResolution);
            var index = (int)MathF.Floor(MathUtil.Clamp(scaledImportance, maxResolutionIndex, atlas.LayerCount - 1));
            ImGui.Text($"index: {index}, maxResolutionIndex: {maxResolutionIndex}, scaledImportance: {scaledImportance}, distanceImportance: {distanceImportance}");
        }
    }
}