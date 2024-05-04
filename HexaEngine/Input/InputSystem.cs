namespace HexaEngine.Input
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Scenes;

    public class InputSystem : ISceneSystem
    {
        public string Name { get; } = "Input System";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.EarlyUpdate | SystemFlags.Destroy;

        public void Awake(Scene scene)
        {
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
            if (Platform.AppConfig == null || !Platform.AppConfig.Variables.TryGetValue("InputMap", out var xml))
            {
                return;
            }

            InputMap? map = InputMap.LoadFromText(xml);

            if (map != null)
            {
                Input.Current.ImportFrom(map, true);
            }
        }

        public void Update(float delta)
        {
            Input.Current.Update();
        }

        public void Destroy()
        {
        }
    }
}