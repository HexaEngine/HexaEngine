namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;

    public static class GraphicsManager
    {
        private static IGraphicsDevice graphicsDevice;
        private static RenderQueue queue;
        private static IResourceManager resources;
        private static ILightManager lights;
        private static IPostProcessManager postProcess;

        public static void Initialize(IGraphicsDevice device)
        {
            graphicsDevice = device;
            queue = new();
        }

        public static RenderQueue Queue => queue;

        public static IResourceManager Resources => resources;

        public static ILightManager Lights => lights;

        public static IPostProcessManager PostProcess => postProcess;
    }

    public interface IInstanceManger
    {
    }
}