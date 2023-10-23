namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.ImGuiNET;
    using HexaEngine.Projects;

    public class GitWidget : EditorWindow
    {
        private string commitMessage = string.Empty;
        private bool commitAmend;

        protected override string Name { get; } = "Git";

        public override void DrawContent(IGraphicsContext context)
        {
            if (ProjectVersionControl.Repository == null)
            {
                if (ProjectManager.Project != null)
                {
                    if (ImGui.Button("Init Git Repo"))
                    {
                        ProjectVersionControl.Init();
                    }
                }

                return;
            }

            if (ImGui.Button("Fetch"))
            {
                //ProjectVersionControl.Fetch();
            }

            ImGui.InputTextMultiline("Commit Message:", ref commitMessage, 1024);
            if (ImGui.Button("Commit All"))
            {
                if (!string.IsNullOrWhiteSpace(commitMessage))

                {
                    ProjectVersionControl.CommitChanges(commitMessage, commitAmend);
                }
            }

            ImGui.SameLine();
            ImGui.Checkbox("Amend", ref commitAmend);

            if (ImGui.BeginListBox("Changes"))
            {
                lock (ProjectVersionControl.SyncObject)
                {
                    foreach (string file in ProjectVersionControl.GetChangedFiles())
                    {
                        ImGui.Text(file);
                    }
                }
            }

            ImGui.EndListBox();
        }
    }
}