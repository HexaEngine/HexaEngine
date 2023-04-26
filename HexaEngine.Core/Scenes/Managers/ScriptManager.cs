namespace HexaEngine.Core.Scenes.Managers
{
    using BepuUtilities;
    using HexaEngine.Core.Scenes;
    using System.Collections.Generic;

    public class ScriptManager : ISystem
    {
        private readonly List<IScriptComponent> scripts = new();

        public IReadOnlyList<IScriptComponent> Scripts => scripts;

        public string Name => "Scripts";

        public void Register(GameObject gameObject)
        {
            scripts.AddComponentIfIs(gameObject);
        }

        public void Unregister(GameObject gameObject)
        {
            scripts.RemoveComponentIfIs(gameObject);
        }

        public void Update(ThreadDispatcher dispatcher)
        {
            if (Application.InDesignMode)
            {
                return;
            }

            for (int i = 0; i < scripts.Count; i++)
            {
                scripts[i].Update();
            }
            //dispatcher.DispatchWorkers(Update, scripts.Count);
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
            if (Application.InDesignMode)
            {
                return;
            }

            dispatcher.DispatchWorkers(FixedUpdate, scripts.Count);
        }

        private void FixedUpdate(int i)
        {
            scripts[i].FixedUpdate();
        }

        public void Awake(ThreadDispatcher dispatcher)
        {
        }

        public void Destroy(ThreadDispatcher dispatcher)
        {
        }
    }
}