namespace HexaEngine.Scenes
{
    public enum PrefabFlags
    {
        None,
        // for later use.
    }

    public class Prefab
    {
        private readonly SceneRootNode root;
        private string? path;
        private PrefabFlags flags;

        public Prefab()
        {
            root = new(null!);
        }

        public Prefab(SceneRootNode root)
        {
            this.root = root;
        }

        public Prefab(Scene scene) : this((SceneRootNode)scene.Root)
        {
        }

        public SceneRootNode Root => root;

        public PrefabFlags Flags { get => flags; set => flags = value; }

        [JsonIgnore]
        public string? Path { get => path; internal set => path = value; }

        public void BuildReferences()
        {
            root.BuildReferences();
        }

        public Scene ToScene()
        {
            Scene scene = new(root);
            scene.IsPrefabScene = true;
            scene.Path = path;

            return scene;
        }
    }
}