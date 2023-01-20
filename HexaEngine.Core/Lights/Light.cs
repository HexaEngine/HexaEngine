namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Scenes;
    using System.Numerics;

    public abstract class Light : GameObject
    {
        public bool Updated;
        protected const float DegToRadFactor = 0.0174532925f;
        protected Vector4 color = Vector4.One;

        [EditorProperty("Color", EditorPropertyMode.Colorpicker)]
        public Vector4 Color { get => color; set => color = value; }

        [EditorProperty("Cast Shadows")]
        public bool CastShadows { get; set; }

        public abstract LightType LightType { get; }
    }
}