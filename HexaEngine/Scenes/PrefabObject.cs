namespace HexaEngine.Scenes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Logging;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes.Serialization;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [Guid("86689502-92C6-4401-829A-5F022C1753D9")]
    [EditorGameObject<PrefabObject>("Prefab Object")]
    public class PrefabObject : GameObject
    {
        private AssetRef prefabAsset;
        private Prefab? prefab;

        [EditorProperty("Prefab", AssetType.Prefab)]
        public AssetRef PrefabAsset
        {
            get => prefabAsset;
            set
            {
                prefabAsset = value;

                if (!Initialized)
                {
                    return;
                }

                UnloadPrefab();
                LoadPrefab(false);
            }
        }

        public Prefab? Prefab => prefab;

        public override void Initialize()
        {
            LoadPrefab(true);

            base.Initialize();
        }

        public void LoadPrefab(bool init)
        {
            if (prefab != null)
            {
                return;
            }

            if (prefabAsset.Guid == Guid.Empty)
            {
                return;
            }

            var prefabArtifact = prefabAsset.GetMetadata();

            if (prefabArtifact == null || prefabArtifact.Type != AssetType.Prefab)
            {
                LoggerFactory.General.Error($"Failed to load prefab, {prefabAsset.Guid}");
                return;
            }

            if (!PrefabSerializer.TryDeserialize(prefabArtifact.Path, out prefab, out _))
            {
                LoggerFactory.General.Error($"Failed to load prefab, {prefabAsset.Guid}");
                return;
            }

            for (int i = 0; i < prefab.Root.Components.Count; i++)
            {
                var component = prefab.Root.Components[i];
                component.Guid = Guid.NewGuid();
                if (init)
                {
                    Components.Add(component);
                }
                else
                {
                    AddComponent(component);
                }
            }

            for (int i = 0; i < prefab.Root.Children.Count; i++)
            {
                var child = prefab.Root.Children[i];
                child.Guid = Guid.NewGuid();
                if (init)
                {
                    Children.Add(child);
                }
                else
                {
                    AddChild(child);
                }
            }

            if (!Application.InEditMode)
            {
                Transform.Position += prefab.Root.Transform.Position;
                Transform.Orientation *= prefab.Root.Transform.Orientation;
                Transform.Scale *= prefab.Root.Transform.Scale;
            }
        }

        public override void Uninitialize()
        {
            base.Uninitialize();

            UnloadPrefab();
        }

        public void UnloadPrefab()
        {
            if (prefab == null)
            {
                return;
            }

            for (int i = 0; i < prefab.Root.Components.Count; i++)
            {
                var component = prefab.Root.Components[i];
                RemoveComponent(component);
            }

            for (int i = 0; i < prefab.Root.Children.Count; i++)
            {
                var child = prefab.Root.Children[i];
                RemoveChild(child);
            }

            prefab = null;
        }
    }
}