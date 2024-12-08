﻿namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Projects;
    using System.Numerics;

    public class ErrorListWidget : EditorWindow
    {
        public ErrorListWidget()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.MenuBar;
        }

        protected override string Name { get; } = "Error List";

        public override void DrawWindow(IGraphicsContext context)
        {
            if (!IsShown)
            {
                return;
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0));
            if (!ImGui.Begin(Name, ref isShown, Flags))
            {
                ImGui.PopStyleVar(1);
                if (wasShown)
                {
                    OnClosed();
                }
                wasShown = false;
                ImGui.End();
                return;
            }
            ImGui.PopStyleVar(1);

            if (!wasShown)
            {
                OnShown();
            }
            wasShown = true;

            windowEnded = false;

            DrawContent(context);

            if (!windowEnded)
            {
                ImGui.End();
            }
        }

        private bool showErrors = true;
        private bool showWarnings = true;
        private bool showMessages = true;
        private string searchString = string.Empty;
        private readonly HashSet<string> codesFilter = [];
        private readonly HashSet<string> projectsFilter = [];
        private readonly HashSet<string> filesFilter = [];

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            var compilation = ProjectManager.CompilationResult;
            if (compilation == null)
            {
                return;
            }

            if (ImGui.BeginMenuBar())
            {
                ImGui.TextColored(Colors.Crimson, $"{UwU.Xmark}");

                if (ImGui.MenuItem($"{compilation.ErrorCount} Errors", !showErrors))
                {
                    showErrors = !showErrors;
                }

                ImGui.TextColored(Colors.Yellow, $"{UwU.TriangleExclamation}");

                if (ImGui.MenuItem($"{compilation.WarningCount} Warnings", !showWarnings))
                {
                    showWarnings = !showWarnings;
                }

                ImGui.TextColored(Colors.LightBlue, $"{UwU.CircleExclamation}");

                if (ImGui.MenuItem($"{compilation.MessageCount} Messages", !showMessages))
                {
                    showMessages = !showMessages;
                }

                if (ImGui.MenuItem($"{UwU.FilterCircleXmark}"))
                {
                    showErrors = showWarnings = showMessages = true;
                    codesFilter.Clear();
                    projectsFilter.Clear();
                    filesFilter.Clear();
                    searchString = string.Empty;
                }
                ImGui.SetItemTooltip("Clear All Filters");

                var avail = ImGui.GetContentRegionAvail();
                var cursor = ImGui.GetCursorPos();
                const float filterWidth = 200;
                ImGui.SetCursorPos(new Vector2(cursor.X + avail.X - filterWidth, cursor.Y));
                ImGui.SetNextItemWidth(filterWidth);
                ImGui.InputTextWithHint("##Filter", "Search ...", ref searchString, 1024);

                ImGui.EndMenuBar();
            }

            if (!ImGui.BeginTable("ErrorTable", 6, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable))
            {
                return;
            }

            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("", 200);
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("");

            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text("Code");
            ImGui.SameLine();
            if (ImGui.SmallButton($"{UwU.Filter}##Code"))
            {
                ImGui.OpenPopup($"{UwU.Filter}##Code");
            }
            if (ImGui.BeginPopupContextItem($"{UwU.Filter}##Code", ImGuiPopupFlags.None))
            {
                if (ImGui.MenuItem("Clear Filter"))
                {
                    codesFilter.Clear();
                }
                ImGui.Separator();
                if (ImGui.BeginListBox("Codes"))
                {
                    for (int i = 0; i < compilation.Codes.Count; i++)
                    {
                        var code = compilation.Codes[i];
                        var selected = !codesFilter.Contains(code);
                        if (ImGui.Checkbox(code, ref selected))
                        {
                            if (selected)
                            {
                                codesFilter.Remove(code);
                            }
                            else
                            {
                                codesFilter.Add(code);
                            }
                        }
                    }
                    ImGui.EndListBox();
                }
                ImGui.EndPopup();
            }

            ImGui.TableSetColumnIndex(2);
            ImGui.Text("Description");
            ImGui.TableSetColumnIndex(3);
            ImGui.Text("Project");
            ImGui.SameLine();
            if (ImGui.SmallButton($"{UwU.Filter}##Project"))
            {
                ImGui.OpenPopup($"{UwU.Filter}##Project");
            }
            if (ImGui.BeginPopupContextItem($"{UwU.Filter}##Project", ImGuiPopupFlags.None))
            {
                if (ImGui.MenuItem("Clear Filter"))
                {
                    projectsFilter.Clear();
                }
                ImGui.Separator();
                if (ImGui.BeginListBox("Projects"))
                {
                    for (int i = 0; i < compilation.Projects.Count; i++)
                    {
                        var code = compilation.Projects[i];
                        var selected = !projectsFilter.Contains(code);
                        if (ImGui.Checkbox(code, ref selected))
                        {
                            if (selected)
                            {
                                projectsFilter.Remove(code);
                            }
                            else
                            {
                                projectsFilter.Add(code);
                            }
                        }
                    }
                    ImGui.EndListBox();
                }
                ImGui.EndPopup();
            }
            ImGui.TableSetColumnIndex(4);
            ImGui.Text("File");
            ImGui.SameLine();
            if (ImGui.SmallButton($"{UwU.Filter}##File"))
            {
                ImGui.OpenPopup($"{UwU.Filter}##File");
            }
            if (ImGui.BeginPopupContextItem($"{UwU.Filter}##File", ImGuiPopupFlags.None))
            {
                if (ImGui.MenuItem("Clear Filter"))
                {
                    filesFilter.Clear();
                }
                ImGui.Separator();
                if (ImGui.BeginListBox("Files"))
                {
                    for (int i = 0; i < compilation.Files.Count; i++)
                    {
                        var code = compilation.Files[i];
                        var selected = !filesFilter.Contains(code);
                        if (ImGui.Checkbox(code, ref selected))
                        {
                            if (selected)
                            {
                                filesFilter.Remove(code);
                            }
                            else
                            {
                                filesFilter.Add(code);
                            }
                        }
                    }
                    ImGui.EndListBox();
                }
                ImGui.EndPopup();
            }
            ImGui.TableSetColumnIndex(5);
            ImGui.Text("Location");

            for (int i = 0; i < compilation.Messages.Count; i++)
            {
                var message = compilation.Messages[i];

                if (FilterMessage(message))
                {
                    continue;
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Dummy(new(1, 0));
                ImGui.SameLine();
                switch (message.Severity)
                {
                    case MSBuildMessageSeverity.Error:
                        ImGui.TextColored(Colors.Crimson, $"{UwU.Xmark}");
                        break;

                    case MSBuildMessageSeverity.Warning:
                        ImGui.TextColored(Colors.Yellow, $"{UwU.TriangleExclamation}");
                        break;

                    case MSBuildMessageSeverity.Message:
                        ImGui.TextColored(Colors.LightBlue, $"{UwU.CircleExclamation}");
                        break;

                    default:
                        ImGui.TextColored(Colors.LightBlue, $"{UwU.CircleQuestion}");
                        break;
                }

                ImGui.TableSetColumnIndex(1);
                ImGui.Text(message.Code);
                ImGui.TableSetColumnIndex(2);
                ImGui.Text(message.Description);
                ImGui.TableSetColumnIndex(3);
                ImGui.Text(message.Project);
                ImGui.TableSetColumnIndex(4);
                if (ImGui.TextLink(message.FileName))
                {
                    ProjectManager.OpenFileInEditor($"{message.File}##{i}", message.Line, message.Column);
                }
                ImGui.TableSetColumnIndex(5);
                ImGui.Text($"{message.Location}");
            }

            ImGui.EndTable();
        }

        private bool FilterMessage(MSBuildMessage message)
        {
            switch (message.Severity)
            {
                case MSBuildMessageSeverity.Error:
                    if (!showErrors)
                    {
                        return true;
                    }

                    break;

                case MSBuildMessageSeverity.Warning:
                    if (!showWarnings)
                    {
                        return true;
                    }

                    break;

                case MSBuildMessageSeverity.Message:
                    if (!showMessages)
                    {
                        return true;
                    }

                    break;
            }

            if (!string.IsNullOrEmpty(searchString) && !message.Description.Contains(searchString))
            {
                return true;
            }

            if (codesFilter.Contains(message.Code))
            {
                return true;
            }

            if (projectsFilter.Contains(message.Project))
            {
                return true;
            }

            if (filesFilter.Contains(message.File))
            {
                return true;
            }

            return false;
        }
    }
}