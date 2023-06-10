using HexaEngine.Scenes.Components;

namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scripts;
    using ImGuiNET;

    public class CSharpScriptEditor : IObjectEditor
    {
        public string Name => "C# Script";

        public Type Type => typeof(CSharpScriptComponent);

        public object? Instance { get; set; }

        public bool IsEmpty => false;

        public void Dispose()
        {
        }

        public void Draw(IGraphicsContext context)
        {
            if (Instance is not CSharpScriptComponent component)
            {
                return;
            }

            var types = AssemblyManager.GetAssignableTypes(typeof(IScript));
            var names = AssemblyManager.GetAssignableTypeNames(typeof(IScript));

            var type = component.ScriptType != null ? AssemblyManager.GetType(component.ScriptType) : null;

            int index = type != null ? types.IndexOf(type) : -1;

            if (ImGui.Combo("Script", ref index, names, names.Length))
            {
                component.ScriptType = types[index].FullName;
            }
        }
    }
}