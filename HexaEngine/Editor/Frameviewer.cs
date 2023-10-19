namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.UI;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Projects;
    using HexaEngine.Rendering.Renderers;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using ImGuizmoNET;
    using System;
    using System.Numerics;

    public partial class Frameviewer
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
            ImGuizmo.SetOrthographic(CameraManager.Dimension == CameraEditorDimension.Dim2D);
            DebugDraw.SetViewport(Viewport);
        }

        internal unsafe void Draw()
        {
            if (!ImGui.Begin("Scene", ref isShown, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.MenuBar))
            {
                isVisible = false;
                ImGui.End();
                return;
            }
            ImGuizmo.SetDrawlist();
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
                if (CameraManager.Dimension == CameraEditorDimension.Dim3D)
                {
                    if (ImGui.Button("3D"))
                    {
                        CameraManager.Dimension = CameraEditorDimension.Dim2D;
                    }
                }
                else
                {
                    if (ImGui.Button("2D"))
                    {
                        CameraManager.Dimension = CameraEditorDimension.Dim3D;
                    }
                }

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
                                ProjectManager.BuildScripts().Wait();
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
                    ImGui.Text("Shading Mode");

                    SceneRenderer? renderer = SceneRenderer.Current;

                    if (renderer != null)
                    {
                        if (ImGui.RadioButton("Wireframe", renderer.Shading == ViewportShading.Wireframe))
                        {
                            renderer.Shading = ViewportShading.Wireframe;
                        }
                        if (ImGui.RadioButton("Solid", renderer.Shading == ViewportShading.Solid))
                        {
                            renderer.Shading = ViewportShading.Solid;
                        }
                        if (ImGui.RadioButton("Rendered", renderer.Shading == ViewportShading.Rendered))
                        {
                            renderer.Shading = ViewportShading.Rendered;
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            position = ImGui.GetWindowPos();
            size = ImGui.GetWindowSize();

            var windowViewport = ImGui.GetWindowViewport();

            var workPos = windowViewport.WorkPos;

            position -= workPos;

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

            ImGui.PushItemWidth(100);

            var mode = Mode;
            if (ComboEnumHelper<ImGuizmoMode>.Combo("##Mode", ref mode))
            {
                Mode = mode;
            }

            ImGui.PopItemWidth();

            if (ImGui.Button("\xECE9", new(32, 32)))
            {
                Operation = ImGuizmoOperation.Translate;
            }
            TooltipHelper.Tooltip("Translate");

            if (ImGui.Button("\xE7AD", new(32, 32)))
            {
                Operation = ImGuizmoOperation.Rotate;
            }
            TooltipHelper.Tooltip("Rotate");

            if (ImGui.Button("\xE740", new(32, 32)))
            {
                Operation = ImGuizmoOperation.Scale;
            }
            TooltipHelper.Tooltip("Scale");

            if (ImGui.Button("\xE759", new(32, 32)))
            {
                Operation = ImGuizmoOperation.Universal;
            }
            TooltipHelper.Tooltip("Translate & Rotate & Scale");

            InspectorDraw();

            ImGui.End();
        }
    }
}