namespace HexaEngine.Scenes
{
    using HexaEngine.Collections;
    using HexaEngine.Scripts;

    public interface IComponent
    {
        [JsonIgnore]
        public GameObject GameObject { get; set; }

        public void Awake();

        public void Destroy();
    }

    public interface IAudioComponent : IComponent
    {
        public void Update();
    }

    public interface IScriptComponent : IComponent, INotifyFlagsChanged<ScriptFlags>
    {
        public void FixedUpdate()
        {
        }

        public void Update()
        {
        }
    }
}