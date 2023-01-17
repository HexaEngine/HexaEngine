namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Mathematics;
    using HexaEngine.Projects;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using ImGuizmoNET;
    using System;
    using System.Numerics;

    public class Framebuffer
    {
        private readonly IGraphicsDevice device;

        private Vector2 position;
        private Vector2 size;
        private bool isShown;

        public bool IsShown { get => isShown; set => isShown = value; }

        public Vector2 Position => position;

        public Vector2 Size => size;

        public Viewport Viewport;
        public Viewport SourceViewport;

        public static bool Fullframe;

        private static readonly Profiler fpsProfiler = new("latency", () => (int)(1000 / Time.Delta), (x, sb) => { }, 100);

        public Framebuffer(IGraphicsDevice device)
        {
            this.device = device;
        }

        internal void Update()
        {
            ImGuizmo.SetRect(Viewport.X, Viewport.Y, Viewport.Width, Viewport.Height);
        }

        internal void Draw()
        {
            ImGui.Begin("Framebuffer", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.MenuBar);
            var scene = SceneManager.Current;
            if (ImGui.IsWindowHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                GameObject? gameObject = ObjectPickerManager.Select(device.Context, Mouse.Position, Viewport);
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
                        ImGui.BeginDisabled(true);

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
                        ImGui.EndDisabled();
                }

                {
                    bool modeSwitched = false;
                    if (Application.InDesignMode || !scene.IsSimulating)
                        ImGui.BeginDisabled(true);

                    if (ImGui.Button("\xE769"))
                    {
                        scene.IsSimulating = false;
                        modeSwitched = true;
                    }

                    if ((Application.InDesignMode || !scene.IsSimulating) && !modeSwitched)
                        ImGui.EndDisabled();
                }

                {
                    bool modeSwitched = false;
                    if (Application.InDesignMode)
                        ImGui.BeginDisabled(true);

                    if (ImGui.Button("\xE71A"))
                    {
                        scene.IsSimulating = false;
                        SceneManager.BeginReload();
                        scene.RestoreState();
                        Application.InDesignMode = true;
                        SceneManager.EndReload();
                        modeSwitched = true;
                    }

                    if (Application.InDesignMode && !modeSwitched)
                        ImGui.EndDisabled();
                }

                int cameraIndex = scene.ActiveCamera;
                if (ImGui.Combo("Current Camera", ref cameraIndex, scene.CameraNames, scene.Cameras.Count))
                {
                    scene.ActiveCamera = cameraIndex;
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

            fpsProfiler.Draw();

            ImGui.End();
        }
    }
}