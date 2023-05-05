namespace HexaEngine.Core.Scenes.Managers
{
    using HexaEngine.Core.Collections;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scripts;

    public class ScriptManager : ISystem
    {
        private readonly FlaggedList<ScriptFlags, IScriptComponent> scripts = new();

        public string Name => "Scripts";

        public SystemFlags Flags { get; } = SystemFlags.Update | SystemFlags.FixedUpdate;

        public IReadOnlyList<IScriptComponent> Scripts => (IReadOnlyList<IScriptComponent>)scripts;

        public void Register(GameObject gameObject)
        {
            scripts.AddComponentIfIs<IScriptComponent>(gameObject);
        }

        public void Unregister(GameObject gameObject)
        {
            scripts.RemoveComponentIfIs<IScriptComponent>(gameObject);
        }

        public void Update(float delta)
        {
            if (Application.InDesignMode)
            {
                return;
            }

            var scriptList = scripts[ScriptFlags.Update];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Update();
            }
        }

        public void FixedUpdate()
        {
            if (Application.InDesignMode)
            {
                return;
            }

            var scriptList = scripts[ScriptFlags.FixedUpdate];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Update();
            }
        }

        public void Awake()
        {
            var scriptList = scripts[ScriptFlags.Awake];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Awake();
            }
        }

        public void Destroy()
        {
            var scriptList = scripts[ScriptFlags.Destroy];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Destory();
            }
        }
    }
}