namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Scripting;
    using System.Collections.Generic;

    public class ScriptManager
    {
        private readonly List<IScript> scripts = new();

        public IReadOnlyList<IScript> Scripts => scripts;

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

        public void FixedUpdate()
        {
            for (int i = 0; i < scripts.Count; i++)
            {
                scripts[i].FixedUpdate();
            }
        }
    }
}