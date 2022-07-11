namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System;
    using System.Numerics;

    public abstract class Light : SceneNode
    {
        protected const float DegToRadFactor = 0.0174532925f;
        protected Vector4 color;

        [EditorProperty("Color", EditorPropertyMode.Colorpicker)]
        public Vector4 Color { get => color; set => color = value; }

        [EditorProperty("Cast Shadows")]
        public bool CastShadows { get; set; }

        public abstract LightType Type { get; }

        public abstract void ClearDepth(IGraphicsContext context);

        public abstract void BindDepth(IGraphicsContext context, int startSlot);

        public abstract void RenderDepth(IGraphicsContext context, Action<IGraphicsContext, Viewport, IView> callback);

        public abstract void Render(IGraphicsContext context, Viewport viewport, IView view, IView scene, int indexCount);

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