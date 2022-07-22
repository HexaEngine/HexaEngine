namespace HexaEngine.Lights
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using System.Numerics;

    public abstract class Light : SceneNode
    {
        protected const float DegToRadFactor = 0.0174532925f;
        protected Vector4 color = Vector4.One;

        [EditorProperty("Color", EditorPropertyMode.Colorpicker)]
        public Vector4 Color { get => color; set => color = value; }

        [EditorProperty("Cast Shadows")]
        public bool CastShadows { get; set; }

        public abstract LightType Type { get; }
    }
}