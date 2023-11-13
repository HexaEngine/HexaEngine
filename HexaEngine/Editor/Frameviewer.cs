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
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGuizmo;
    using System;
    using System.Numerics;

    public partial class Frameviewer
    {
        private readonly IGraphicsDevice device;
        private bool isShown;
        private bool isVisible;

        public bool IsShown { get => isShown; set => isShown = value; }

        public bool IsVisible => isVisible;

        public Viewport RenderViewport;
        public Viewport ImGuiViewport;
        public Viewport SourceViewport;

        public static bool Fullframe;

        public Frameviewer(IGraphicsDevice device)
        {
            this.device = device;
        }

        internal void Update()
        {
            ImGuizmo.SetRect(ImGuiViewport.X, ImGuiViewport.Y, ImGuiViewport.Width, ImGuiViewport.Height);
            ImGuizmo.SetOrthographic(CameraManager.Dimension == CameraEditorDimension.Dim2D);
            DebugDraw.SetViewport(RenderViewport);
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
                GameObject? gameObject = ObjectPickerManager.SelectObject(device.Context, Mouse.Position, RenderViewport);
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

                if (ImGui.BeginMenu("\xE713"))
                {
                    ImGui.Checkbox("Fullscreen", ref Fullframe);

                    ImGui.Separator();

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

            Vector2 vMin = ImGui.GetWindowContentRegionMin();
            Vector2 vMax = ImGui.GetWindowContentRegionMax();
            Vector2 wPos = ImGui.GetWindowPos();

            var viewport = ImGui.GetWindowViewport();

            Vector2 rVMin = vMin + (wPos - viewport.Pos);
            Vector2 rVMax = vMax + (wPos - viewport.Pos);

            Vector2 iVMin = vMin + wPos;
            Vector2 iVMax = vMax + wPos;

            Vector2 rPosition = rVMin;
            Vector2 rSize = rVMax - rVMin;

            Vector2 iPosition = iVMin;
            Vector2 iSize = iVMax - iVMin;

            if (Fullframe)
            {
                ImGuiViewport = RenderViewport = SourceViewport;
            }
            else
            {
                RenderViewport = Viewport.ScaleAndCenterToFit(SourceViewport, rPosition, rSize);
                ImGuiViewport = Viewport.ScaleAndCenterToFit(SourceViewport, iPosition, iSize);
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