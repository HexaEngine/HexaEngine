namespace HexaEngine.Input
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Scenes;

    public class InputSystem : ISceneSystem
    {
        private InputManager inputManager;

        public string Name { get; } = "Input System";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.EarlyUpdate | SystemFlags.Destroy;

        public void Awake(Scene scene)
        {
            Input.Current = inputManager = new();
            if (!scene.Variables.TryGetValue("InputMapAsset", out var guidString) || !Guid.TryParse(guidString, out var assetGuid))
            {
                LoadFromAppConfig();
                return;
            }

            AssetRef assetRef = new(assetGuid);
            InputMap? map = InputMap.Load(assetRef);

            if (map != null)
            {
                Input.Current.ImportFrom(map, true);
            }
        }

        private static void LoadFromAppConfig()
        {
            if (Platform.AppConfig == null)
            {
                return;
            }

            InputMap map = Platform.AppConfig.InputMap;

            if (map != null)
            {
                Input.Current.ImportFrom(map, true);
            }
        }

        public void Update(float delta)
        {
            inputManager.Update();
        }

        public void Destroy()
        {
            var tmp = Input.Current;
            Input.Current = null;
            tmp.Dispose();
        }
    }
}