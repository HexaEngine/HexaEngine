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

        /*

        /// <summary>
        /// Tests a BoundingBox for intersection.
        /// </summary>
        /// <returns>returns false if shape is not in frustum, true when shape is in the frustum.</returns>
        public abstract bool CullTest(BoundingBox box);

        /// <summary>
        /// Tests a BoundingSphere for intersection.
        /// </summary>
        /// <returns>returns false if shape is not in frustum, true when shape is in the frustum.</returns>
        public abstract bool CullTest(BoundingSphere sphere);
        */
    }
}