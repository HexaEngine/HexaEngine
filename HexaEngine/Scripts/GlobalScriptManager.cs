namespace HexaEngine.Scripts
{
    using HexaEngine.Core;
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
}