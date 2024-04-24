namespace HexaEngine.Lights
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
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
}