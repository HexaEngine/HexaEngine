namespace HexaEngine.Scripts
{
    using HexaEngine.Collections;
    using HexaEngine.Core;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using System.Reflection;

    public class GlobalScriptManager
    {
        private List<GlobalScript> scripts = new();

        static GlobalScriptManager()
        {
            ScriptAssemblyManager.AssembliesUnloaded += AssembliesUnloaded;
            ScriptAssemblyManager.AssemblyLoaded += AssemblyLoaded;
            Application.OnEditorPlayStateTransition += EditorPlayStateTransition;
            Application.OnEditorPlayStateChanged += EditorPlayStateChanged;
            SceneManager.SceneChanged += SceneChanged;
        }

        private static void SceneChanged(object? sender, SceneChangedEventArgs e)
        {
        }

        private static void EditorPlayStateTransition(EditorPlayStateTransitionEventArgs args)
        {
        }

        private static void EditorPlayStateChanged(EditorPlayState newState)
        {
        }

        private static void AssemblyLoaded(object? sender, Assembly e)
        {
        }

        private static void AssembliesUnloaded(object? sender, EventArgs? e)
        {
        }

        public void Load()
        {
        }
    }

    public class ScriptManager : ISceneSystem
    {
        private readonly ComponentTypeQuery<IScriptComponent> components = new();
        private readonly FlaggedList<ScriptFlags, IScriptComponent> scripts = new();
        private bool awaked;

        public string Name => "Scripts";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.Update | SystemFlags.FixedUpdate | SystemFlags.Destroy;

        public IReadOnlyList<IScriptComponent> Scripts => (IReadOnlyList<IScriptComponent>)scripts;

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(components);

            components.OnAdded += OnAdded;
            components.OnRemoved -= OnRemoved;

            for (int i = 0; i < components.Count; i++)
            {
                scripts.Add(components[i]);
                components[i].ScriptCreate();
            }

            for (int i = 0; i < components.Count; i++)
            {
                components[i].ScriptLoad();
            }

            if (Application.InEditMode)
            {
                return;
            }

            awaked = true;

            var scriptList = scripts[ScriptFlags.Awake];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].ScriptAwake();
            }
        }

        private void OnRemoved(GameObject gameObject, IScriptComponent component)
        {
            scripts.Remove(component);

            if (Application.InEditMode || !awaked)
            {
                return;
            }

            component.Destroy();
        }

        private void OnAdded(GameObject gameObject, IScriptComponent component)
        {
            scripts.Add(component);

            if (Application.InEditMode || !awaked)
            {
                return;
            }

            component.ScriptCreate();
            component.ScriptLoad();

            if ((component.Flags & ScriptFlags.Awake) != 0)
            {
                component.ScriptAwake();
            }
        }

        public void Destroy()
        {
            components.OnAdded -= OnAdded;
            components.OnRemoved -= OnRemoved;

            components.Dispose();

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
                scriptList[i].FixedUpdate();
            }
        }
    }
}