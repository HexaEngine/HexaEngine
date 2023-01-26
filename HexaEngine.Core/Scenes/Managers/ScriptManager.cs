namespace HexaEngine.Core.Scenes.Managers
{
    using BepuUtilities;
    using HexaEngine.Core.Scenes;
    using System.Collections.Generic;

    public class ScriptManager : ISystem
    {
        private readonly List<IScriptComponent> scripts = new();

        public IReadOnlyList<IScriptComponent> Scripts => scripts;

        public void Register(GameObject gameObject)
        {
            scripts.AddComponentIfIs(gameObject);
        }

        public void Unregister(GameObject gameObject)
        {
            scripts.RemoveComponentIfIs(gameObject);
        }

        public void Update()
        {
            for (int i = 0; i < scripts.Count; i++)
            {
                scripts[i].Update();
            }
        }

        public void Update(ThreadDispatcher dispatcher)
        {
            dispatcher.DispatchWorkers(Update, scripts.Count);
        }

        private void Update(int i)
        {
            scripts[i].Update();
        }

        public void FixedUpdate()
        {
            for (int i = 0; i < scripts.Count; i++)
            {
                scripts[i].FixedUpdate();
            }
        }

        public void FixedUpdate(ThreadDispatcher dispatcher)
        {
            dispatcher.DispatchWorkers(i => scripts[i].FixedUpdate(), scripts.Count);
        }

        public void Awake(ThreadDispatcher dispatcher)
        {
        }

        public void Destroy(ThreadDispatcher dispatcher)
        {
        }
    }
}