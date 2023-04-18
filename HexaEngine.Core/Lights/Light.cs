namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public abstract class Light : GameObject
    {
        internal bool InUpdateQueue;
        internal uint QueueIndex;
        protected const float DegToRadFactor = 0.0174532925f;
        protected Vector4 color = Vector4.One;
        private bool castShadows;

        public abstract LightType LightType { get; }

        [EditorProperty("Color", EditorPropertyMode.Colorpicker)]
        public Vector4 Color { get => color; set => SetAndNotifyWithEqualsTest(ref color, value); }

        [EditorProperty("Cast Shadows")]
        public bool CastShadows { get => castShadows; set => SetAndNotifyWithEqualsTest(ref castShadows, value); }

        public abstract bool HasShadowMap { get; }

        /// <summary>
        /// Tests if an object that moved affects the shadow volume
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public abstract bool IntersectFrustum(BoundingBox box);

        public abstract IShaderResourceView? GetShadowMap();

        public abstract void CreateShadowMap(IGraphicsDevice device);

        public abstract void DestroyShadowMap();

        public uint GetQueueIndex() => QueueIndex;
    }
}