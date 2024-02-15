namespace HexaEngine.Scenes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor.Attributes;

    [EditorGameObject<PrefabObject>("Prefab Object")]
    public class PrefabObject : GameObject
    {
        private AssetRef prefabAsset;
        private Prefab? prefab;

        [EditorProperty("Prefab", AssetType.Prefab)]
        public AssetRef PrefabAsset { get => prefabAsset; set => prefabAsset = value; }

        [JsonIgnore]
        public IReadOnlyPrefab? Prefab => prefab;

        public override void Initialize()
        {
            var prefabArtifact = ArtifactDatabase.GetArtifact(prefabAsset.Guid);
            if (prefabArtifact == null || prefabArtifact.Type != AssetType.Prefab)
            {
                Logger.Error($"Failed to load prefab, {prefabAsset.Guid}");
                return;
            }

            if (!Scenes.Prefab.TryDeserialize(prefabArtifact.Path, out prefab, out _))
            {
                Logger.Error($"Failed to load prefab, {prefabAsset.Guid}");
                return;
            }

            Components.AddRange(prefab.Components);
            Children.AddRange(prefab.Children);

            if (!Application.InEditMode)
            {
                Transform.Position += prefab.Transform.Position;
                Transform.Orientation *= prefab.Transform.Orientation;
                Transform.Scale *= prefab.Transform.Scale;
            }

            base.Initialize();
        }

        public override void Uninitialize()
        {
            base.Uninitialize();

            Components.Clear();
            Children.Clear();
        }
    }
}