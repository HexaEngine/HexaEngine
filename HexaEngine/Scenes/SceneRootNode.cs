#define PROFILE

namespace HexaEngine.Scenes
{
    using HexaEngine.Scenes.Serialization;
    using Newtonsoft.Json;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Scenes.Scene+SceneRootNode")]
    public class SceneRootNode : GameObject
    {
        private Scene parent;

        public SceneRootNode(Scene parent)
        {
            this.parent = parent;
            Name = "Root";
        }

        [JsonIgnore]
        public Scene Scene { get => parent; internal protected set => parent = value; }

        public override Scene GetScene()
        {
            return parent;
        }

        public override void GetDepth(ref int depth)
        {
            return;
        }
    }
}