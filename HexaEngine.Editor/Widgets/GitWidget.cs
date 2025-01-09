namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Projects;
    using LibGit2Sharp;

    public class GitWidget : EditorWindow
    {
        private string commitMessage = string.Empty;
        private bool commitAmend;

        protected override string Name { get; } = $"{UwU.GitAlt} Git";

        public override void DrawContent(IGraphicsContext context)
        {
            if (ProjectVersionControl.Repository == null)
            {
                if (ProjectManager.Loaded)
                {
                    if (ImGui.Button("Init Git Repo"u8))
                    {
                        ProjectVersionControl.Init();
                    }
                }

                return;
            }

            if (ImGui.BeginCombo("##Branches"u8, ProjectVersionControl.HeadName))
            {
                ImGui.SeparatorText("Local Branches"u8);

                foreach (var branch in ProjectVersionControl.GetLocalBranches())
                {
                    if (ImGui.Selectable(branch.FriendlyName, ProjectVersionControl.HeadName == branch.FriendlyName))
                    {
                        CheckoutBranch(branch);
                    }

                    BranchContextMenu(branch);
                }

                ImGui.SeparatorText("Remote Branches"u8);

                foreach (var branch in ProjectVersionControl.GetRemoteBranches())
                {
                    if (ImGui.Selectable(branch.FriendlyName, ProjectVersionControl.HeadName == branch.FriendlyName))
                    {
                        CheckoutBranch(branch);
                    }

                    BranchContextMenu(branch);
                }

                ImGui.EndCombo();
            }

            if (ImGui.Button("\xE74B"u8))
            {
                ProjectVersionControl.Fetch();
            }

            ImGui.SameLine();

            if (ImGui.Button("\xE896"u8))
            {
                ProjectVersionControl.Pull();
            }

            ImGui.SameLine();

            if (ImGui.Button("\xE898"u8))
            {
                ProjectVersionControl.Push();
            }

            ImGui.SameLine();

            if (ImGui.Button("\xE895"u8))
            {
                ProjectVersionControl.Sync();
            }

            ImGui.SameLine();

            ImGui.Text(ProjectVersionControl.CommitDifference.Text);

            ImGui.InputTextMultiline("Commit Message:"u8, ref commitMessage, 1024);
            if (ImGui.Button("Commit All"u8))
            {
                if (!string.IsNullOrWhiteSpace(commitMessage))

                {
                    ProjectVersionControl.CommitChanges(commitMessage, commitAmend);
                    commitMessage = string.Empty;
                }
            }

            ImGui.SameLine();
            ImGui.Checkbox("Amend"u8, ref commitAmend);

            if (ImGui.BeginListBox("Changes"u8))
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

        private static void BranchContextMenu(Branch branch)
        {
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Checkout"u8))
                {
                    CheckoutBranch(branch);
                }

                ImGui.Separator();

                ImGui.BeginDisabled(ProjectVersionControl.Head.FriendlyName == branch.FriendlyName);

                if (ImGui.MenuItem("\xEA3C Merge into Current Branch"u8))
                {
                    MessageBox.Show("Merge Branch", "Are you sure you want to merge the branch with HEAD?", null, (s, e) =>
                    {
                        if (s.Result == MessageBoxResult.Yes)
                        {
                            var result = ProjectVersionControl.MergeBranch(branch);
                            if (result?.Status == MergeStatus.Conflicts)
                            {
                                MessageBox.Show("Merge Conflicts", "Merge conflicts found!");
                            }
                        }
                    }, MessageBoxType.YesNo);
                }

                if (ImGui.MenuItem("\xE8B5 Rebase Current Branch onto"u8))
                {
                    MessageBox.Show("Rebase Branch", "Are you sure you want to rebase HEAD with the branch?", null, (s, e) =>
                    {
                        if (s.Result == MessageBoxResult.Yes)
                        {
                            var result = ProjectVersionControl.Rebase(branch, ProjectVersionControl.Head);
                            if (result?.Status == RebaseStatus.Conflicts)
                            {
                                MessageBox.Show("Rebase Conflicts", "Rebase conflicts found!");
                                ProjectVersionControl.RebaseAbort();
                            }
                            ProjectVersionControl.RebaseContinue();
                        }
                    }, MessageBoxType.YesNo);
                }

                ImGui.EndDisabled();

                ImGui.Separator();

                if (ImGui.MenuItem("\xE8AC Rename"u8))
                {
                }

                if (ImGui.MenuItem("\xE711 Delete"u8))
                {
                    MessageBox.Show("Delete Branch", "Are you sure you want to delete the branch?", null, (s, e) =>
                    {
                        if (s.Result == MessageBoxResult.Yes)
                        {
                            ProjectVersionControl.DeleteBranch(branch);
                        }
                    }, MessageBoxType.YesNo);
                }

                ImGui.EndPopup();
            }
        }

        private static void CheckoutBranch(Branch branch)
        {
            MessageBox.Show("Checkout Branch", "Are you sure you want to checkout the branch,\n all untracked changes will be lost", null, (s, e) =>
            {
                if (s.Result == MessageBoxResult.Yes)
                {
                    ProjectVersionControl.CheckoutBranch(branch);
                }
            }, MessageBoxType.YesNo);
        }
    }
}