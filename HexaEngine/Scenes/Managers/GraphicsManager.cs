namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Rendering;

    public static class GraphicsManager
    {
#pragma warning disable CS8618 // Non-nullable field 'graphicsDevice' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private static IGraphicsDevice graphicsDevice;
#pragma warning restore CS8618 // Non-nullable field 'graphicsDevice' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'queue' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private static RenderQueue queue;
#pragma warning restore CS8618 // Non-nullable field 'queue' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'resources' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS0649 // Field 'GraphicsManager.resources' is never assigned to, and will always have its default value null
        private static IResourceManager resources;
#pragma warning restore CS0649 // Field 'GraphicsManager.resources' is never assigned to, and will always have its default value null
#pragma warning restore CS8618 // Non-nullable field 'resources' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS0649 // Field 'GraphicsManager.postProcess' is never assigned to, and will always have its default value null
#pragma warning disable CS8618 // Non-nullable field 'postProcess' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private static IPostProcessManager postProcess;
#pragma warning restore CS8618 // Non-nullable field 'postProcess' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS0649 // Field 'GraphicsManager.postProcess' is never assigned to, and will always have its default value null

        public static void Initialize(IGraphicsDevice device)
        {
            graphicsDevice = device;
            queue = new();
        }

        public static RenderQueue Queue => queue;

        public static IResourceManager Resources => resources;

        public static IPostProcessManager PostProcess => postProcess;
    }
}