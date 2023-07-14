namespace HexaEngine.Scenes
{
    using HexaEngine.Collections;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Scripts;

    public interface IComponent
    {
        public void Awake(IGraphicsDevice device, GameObject gameObject);

        public void Destroy();
    }

    public interface IAudioComponent : IComponent
    {
        public void Update();
    }

    public interface IScriptComponent : IComponent, INotifyFlagsChanged<ScriptFlags>
    {
        public void Awake()
        {
        }

        public void FixedUpdate()
        {
        }

        public void Update()
        {
        }
    }
}