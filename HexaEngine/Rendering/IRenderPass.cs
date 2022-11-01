namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;

    public interface IRenderPass
    {
        public string Name { get; }

        public int Priority { get; }

        public event EventHandler? StateChanged;

        public void Initialize(IGraphicsDevice device, int width, int height, RenderPassCollection passes);

        /// <summary>
        /// The Pass should release resources that are linked to the size
        /// </summary>
        public void ResizeBegin();

        /// <summary>
        /// The Pass may recreate the released resources.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ResizeEnd(IGraphicsDevice device, int width, int height, RenderPassCollection passes);

        /// <summary>
        /// The Pass should update all CB VB IB here.
        /// </summary>
        /// <param name="context"></param>
        public void Update(IGraphicsContext context, Scene scene);

        /// <summary>
        /// Only called once during recording.
        /// </summary>
        /// <param name="context"></param>
        public void Draw(IGraphicsContext context, Scene scene, Viewport viewport);
    }
}