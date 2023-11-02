namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Scenes;
    using Hexa.NET.ImGui;

    public class SceneVariablesWindow : EditorWindow
    {
        private string _newKeyName = string.Empty;
        protected override string Name => "Scene Variables";

        public override void DrawContent(IGraphicsContext context)
        {
            Scene? scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }

            var vars = scene.Variables;

            if (ImGui.InputText("New key name", ref _newKeyName, 512))
            {
            }
            ImGui.SameLine();
            if (ImGui.SmallButton("add"))
            {
                if (!vars.ContainsKey(_newKeyName))
                {
                    vars.Add(_newKeyName, string.Empty);
                }
            }

            if (ImGui.BeginListBox("Variables"))
            {
                for (int i = 0; i < vars.Count; i++)
                {
                    var pair = vars[i];
                    var key = pair.Key;
                    var value = pair.Value;
                    if (ImGui.InputText(key, ref value, 512))
                    {
                        vars[key] = value;
                    }
                }
                ImGui.EndListBox();
            }
        }
    }
}