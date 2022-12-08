namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
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

        private static readonly Profiler fpsProfiler = new("latency", () => Time.Delta, x => $"{x * 1000:n4}ms\n({1000 / Time.Delta:n0}fps)", 100);

        public Framebuffer(IGraphicsDevice device)
        {
            this.device = device;
        }

        internal void Update(IGraphicsContext context)
        {
            ImGuizmo.SetRect(Viewport.X, Viewport.Y, Viewport.Width, Viewport.Height);
        }

        internal void Draw()
        {
            ImGui.Begin("Framebuffer", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.MenuBar);
            var scene = SceneManager.Current;
            if (scene != null && ImGui.BeginMenuBar())
            {
                // Play "\xE769"
                // Pause "\xE768"
                // Stop "xE71A"
                {
                    bool modeSwitched = false;
                    if (!Designer.InDesignMode && scene.IsSimulating)
                        ImGui.BeginDisabled(true);

                    if (ImGui.Button("\xE768"))
                    {
                        if (Designer.InDesignMode)
                        {
                            scene.IsSimulating = false;
                            SceneManager.BeginReload();
                            scene.SaveState();
                            Designer.InDesignMode = false;
                            scene.IsSimulating = true;
                            SceneManager.EndReload();
                        }
                        else
                        {
                            scene.IsSimulating = true;
                        }
                        modeSwitched = true;
                    }

                    if (!Designer.InDesignMode && scene.IsSimulating && !modeSwitched)
                        ImGui.EndDisabled();
                }

                {
                    bool modeSwitched = false;
                    if (Designer.InDesignMode || !scene.IsSimulating)
                        ImGui.BeginDisabled(true);

                    if (ImGui.Button("\xE769"))
                    {
                        scene.IsSimulating = false;
                        modeSwitched = true;
                    }

                    if ((Designer.InDesignMode || !scene.IsSimulating) && !modeSwitched)
                        ImGui.EndDisabled();
                }

                {
                    bool modeSwitched = false;
                    if (Designer.InDesignMode)
                        ImGui.BeginDisabled(true);

                    if (ImGui.Button("\xE71A"))
                    {
                        scene.IsSimulating = false;
                        SceneManager.BeginReload();
                        scene.RestoreState();
                        Designer.InDesignMode = true;
                        SceneManager.EndReload();
                        modeSwitched = true;
                    }

                    if (Designer.InDesignMode && !modeSwitched)
                        ImGui.EndDisabled();
                }

                int cameraIndex = scene.ActiveCamera;
                if (ImGui.Combo("Current Camera", ref cameraIndex, scene.Cameras.Select(x => x.Name).ToArray(), scene.Cameras.Count))
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