using HexaEngine.Core;

namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Collections;
    using HexaEngine.Scenes;
    using HexaEngine.Scripts;

    public class ScriptManager : ISystem
    {
        private readonly FlaggedList<ScriptFlags, IScriptComponent> scripts = new();
        private bool awaked;

        public string Name => "Scripts";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.Update | SystemFlags.FixedUpdate | SystemFlags.Destroy;

        public IReadOnlyList<IScriptComponent> Scripts => (IReadOnlyList<IScriptComponent>)scripts;

        public void Register(GameObject gameObject)
        {
            scripts.AddComponentIfIs<IScriptComponent>(gameObject, awaked);
        }

        public void Unregister(GameObject gameObject)
        {
            scripts.RemoveComponentIfIs<IScriptComponent>(gameObject, awaked);
        }

        public void Update(float delta)
        {
            if (Application.InEditMode)
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
            if (Application.InEditMode)
            {
                return;
            }

            var scriptList = scripts[ScriptFlags.FixedUpdate];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Update();
            }
        }

        public void Awake(Scene scene)
        {
            if (Application.InEditMode)
            {
                return;
            }

            awaked = true;

            var scriptList = scripts[ScriptFlags.Awake];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Awake();
            }
        }

        public void Destroy()
        {
            if (Application.InEditMode)
            {
                return;
            }

            awaked = false;

            var scriptList = scripts[ScriptFlags.Destroy];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Destroy();
            }
        }
    }
}