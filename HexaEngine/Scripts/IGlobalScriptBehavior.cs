namespace HexaEngine.Scripts
{
    using HexaEngine.Scenes;

    public interface IGlobalScriptBehavior
    {
        public void OnLoad()
        {
        }

        public void OnUnload()
        {
        }

        public void OnSceneLoad(Scene scene)
        {
        }

        public void OnSceneUnload(Scene scene)
        {
        }

        public void Update()
        {
        }

        public void FixedUpdate()
        {
        }
    }
}