namespace HexaEngine.UI
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Scenes;
    using HexaEngine.UI.Graphics;
    using HexaEngine.UI.Graphics.Text;

    public class UISystem : ISceneSystem
    {
        private TextFactory textFactory = null!;
        private readonly UICommandList commandList = new();

        public string Name { get; } = "UI";

        public SystemFlags Flags { get; } = SystemFlags.Load | SystemFlags.Unload | SystemFlags.GraphicsUpdate;

        public static UISystem? Current { get; set; }

        public TextFactory TextFactory => textFactory;

        public UIWindow? Window { get; set; }

        public UICommandList CommandList => commandList;

        public void Load(IGraphicsDevice device)
        {
            textFactory = new TextFactory(device);
        }

        public void GraphicsUpdate(IGraphicsContext context)
        {
            var window = Window;
            if (window == null)
            {
                return;
            }

            window.Render(commandList);
        }

        public void Unload()
        {
            textFactory.Dispose();
            commandList.Dispose();
        }
    }
}