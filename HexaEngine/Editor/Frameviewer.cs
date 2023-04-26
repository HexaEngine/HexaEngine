namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using HexaEngine.Projects;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using ImGuizmoNET;
    using System;
    using System.Numerics;

    public class Frameviewer
    {
        private readonly IGraphicsDevice device;
        private bool isShown;
        private bool isVisible;
        private Vector2 position;
        private Vector2 size;

        public bool IsShown { get => isShown; set => isShown = value; }

        public bool IsVisible => isVisible;

        public Vector2 Position => position;

        public Vector2 Size => size;

        public Viewport Viewport;
        public Viewport SourceViewport;

        public static bool Fullframe;

        public Frameviewer(IGraphicsDevice device)
        {
            this.device = device;
        }

        internal void Update()
        {
            ImGuizmo.SetRect(Viewport.X, Viewport.Y, Viewport.Width, Viewport.Height);
        }

        internal void Draw()
        {
            if (!ImGui.Begin("Scene", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.MenuBar))
            {
                isVisible = false;
                ImGui.End();
                return;
            }
            isVisible = true;
            var scene = SceneManager.Current;
            if (ImGui.IsWindowHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                GameObject? gameObject = ObjectPickerManager.SelectObject(device.Context, Mouse.Position, Viewport);
                if (gameObject != null)
                {
                    if (ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
                    {
                        GameObject.Selected.AddSelection(gameObject);
                    }
                    else
                    {
                        GameObject.Selected.AddOverwriteSelection(gameObject);
                    }
                }
            }

            if (scene != null && ImGui.BeginMenuBar())
            {
                // Play "\xE769"
                // Pause "\xE768"
                // Stop "xE71A"
                {
                    bool modeSwitched = false;
                    if (!Application.InDesignMode && scene.IsSimulating)
                    {
                        ImGui.BeginDisabled(true);
                    }

                    if (ImGui.Button("\xE768"))
                    {
                        if (Application.InDesignMode)
                        {
                            if (ProjectManager.ScriptProjectChanged)
                            {
                                ProjectManager.UpdateScripts().Wait();
                            }
                            SceneManager.Save();
                            scene.IsSimulating = false;
                            SceneManager.BeginReload();
                            scene.SaveState();
                            Application.InDesignMode = false;
                            scene.IsSimulating = true;
                            SceneManager.EndReload();
                        }
                        else
                        {
                            scene.IsSimulating = true;
                        }
                        modeSwitched = true;
                    }

                    if (!Application.InDesignMode && scene.IsSimulating && !modeSwitched)
                    {
                        ImGui.EndDisabled();
                    }
                }

                {
                    bool modeSwitched = false;
                    if (Application.InDesignMode || !scene.IsSimulating)
                    {
                        ImGui.BeginDisabled(true);
                    }

                    if (ImGui.Button("\xE769"))
                    {
                        scene.IsSimulating = false;
                        modeSwitched = true;
                    }

                    if ((Application.InDesignMode || !scene.IsSimulating) && !modeSwitched)
                    {
                        ImGui.EndDisabled();
                    }
                }

                {
                    bool modeSwitched = false;
                    if (Application.InDesignMode)
                    {
                        ImGui.BeginDisabled(true);
                    }

                    if (ImGui.Button("\xE71A") || ImGui.IsKeyDown(ImGuiKey.Escape) && !Application.InDesignMode)
                    {
                        scene.IsSimulating = false;
                        SceneManager.BeginReload();
                        scene.RestoreState();
                        Application.InDesignMode = true;
                        SceneManager.EndReload();
                        modeSwitched = true;
                    }

                    if (Application.InDesignMode && !modeSwitched)
                    {
                        ImGui.EndDisabled();
                    }
                }

                int cameraIndex = scene.ActiveCamera;
                if (ImGui.Combo("##ActiveCamCmbo", ref cameraIndex, scene.CameraNames, scene.Cameras.Count))
                {
                    scene.ActiveCamera = cameraIndex;
                }

                ImGui.Separator();

                if (ImGui.BeginMenu("options"))
                {
                    if (ImGui.RadioButton("Translate", Inspector.Operation == ImGuizmoOperation.Translate))
                    {
                        Inspector.Operation = ImGuizmoOperation.Translate;
                    }

                    if (ImGui.RadioButton("Rotate", Inspector.Operation == ImGuizmoOperation.Rotate))
                    {
                        Inspector.Operation = ImGuizmoOperation.Rotate;
                    }

                    if (ImGui.RadioButton("Scale", Inspector.Operation == ImGuizmoOperation.Scale))
                    {
                        Inspector.Operation = ImGuizmoOperation.Scale;
                    }

                    if (ImGui.RadioButton("Local", Inspector.Mode == ImGuizmoMode.Local))
                    {
                        Inspector.Mode = ImGuizmoMode.Local;
                    }

                    ImGui.SameLine();
                    if (ImGui.RadioButton("World", Inspector.Mode == ImGuizmoMode.World))
                    {
                        Inspector.Mode = ImGuizmoMode.World;
                    }
                    ImGui.Separator();
                    ImGui.Text("Shading Mode");
                    if (ImGui.RadioButton("Wireframe", Application.MainWindow.Renderer.Shading == ViewportShading.Wireframe))
                    {
                        Application.MainWindow.Renderer.Shading = ViewportShading.Wireframe;
                    }
                    if (ImGui.RadioButton("Solid", Application.MainWindow.Renderer.Shading == ViewportShading.Solid))
                    {
                        Application.MainWindow.Renderer.Shading = ViewportShading.Solid;
                    }
                    if (ImGui.RadioButton("Rendered", Application.MainWindow.Renderer.Shading == ViewportShading.Rendered))
                    {
                        Application.MainWindow.Renderer.Shading = ViewportShading.Rendered;
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
            position = ImGui.GetWindowPos();
            size = ImGui.GetWindowSize();

            if (Fullframe)
            {
                Viewport = SourceViewport;
            }
            else
            {
                float ratioX = size.X / SourceViewport.Width;
                float ratioY = size.Y / SourceViewport.Height;
                var s = Math.Min(ratioX, ratioY);
                var w = SourceViewport.Width * s;
                var h = SourceViewport.Height * s;
                var x = (size.X - w) / 2;
                var y = (size.Y - h) / 2;
                Viewport = new(position.X + x, position.Y + y, w, h);
            }

            ImGui.End();
        }
    }
}