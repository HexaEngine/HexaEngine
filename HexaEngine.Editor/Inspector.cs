namespace HexaEngine.Editor
{
    using HexaEngine.Components.Collider;
    using HexaEngine.Core;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using Hexa.NET.ImGuizmo;
    using System.Numerics;
    using HexaEngine.Editor.Icons;
    using Hardware.Info;
    using HexaEngine.Core.Debugging;

    public partial class Frameviewer
    {
        private static bool inspectorEnabled = true;
        private static bool drawGimbal = true;
        private static bool drawGrid = true;
        private static bool drawLights = true;
        private static bool drawLightBounds = false;
        private static bool drawCameras = true;
        private static bool drawSkeletons = true;
        private static bool drawBoundingBoxes = false;
        private static bool drawBoundingSpheres = false;
        private static bool drawColliders = true;

        private static ImGuizmoOperation operation = ImGuizmoOperation.Translate;
        private static ImGuizmoMode mode = ImGuizmoMode.Local;
        private static bool gimbalGrabbed;
        private static Matrix4x4 gimbalBefore;

        public static ImGuizmoOperation Operation { get => operation; set => operation = value; }

        public static ImGuizmoMode Mode { get => mode; set => mode = value; }

        public static bool InspectorEnabled { get => inspectorEnabled; set => inspectorEnabled = value; }

        public static bool DrawGimbal { get => drawGimbal; set => drawGimbal = value; }

        public static bool DrawGrid { get => drawGrid; set => drawGrid = value; }

        public static bool DrawLights { get => drawLights; set => drawLights = value; }

        public static bool DrawLightBounds { get => drawLightBounds; set => drawLightBounds = value; }

        public static bool DrawCameras { get => drawCameras; set => drawCameras = value; }

        public static bool DrawSkeletons { get => drawSkeletons; set => drawSkeletons = value; }

        public static bool DrawColliders { get => drawColliders; set => drawColliders = value; }

        public static bool DrawBoundingBoxes { get => drawBoundingBoxes; set => drawBoundingBoxes = value; }

        public static bool DrawBoundingSpheres { get => drawBoundingSpheres; set => drawBoundingSpheres = value; }

        public static unsafe void InspectorDraw()
        {
            if (!inspectorEnabled)
            {
                return;
            }

            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }

            if (CameraManager.Current == null)
            {
                return;
            }
            var mainCamera = CameraManager.Current;
            var pos = mainCamera.Transform.GlobalPosition;
            var forward = mainCamera.Transform.Forward;

            if (drawGrid)
            {
                if (CameraManager.Dimension == CameraEditorDimension.Dim3D)
                {
                    DebugDraw.DrawGrid("Grid", Matrix4x4.Identity, 100, new Vector4(1, 1, 1, 0.2f));
                }
                else if (CameraManager.Dimension == CameraEditorDimension.Dim2D)
                {
                    DebugDraw.DrawGrid("Grid", MathUtil.RotationYawPitchRoll(0, float.Pi / 2, 0), 100, new Vector4(1, 1, 1, 0.2f));
                }
            }

            if (drawLights)
            {
                for (int i = 0; i < scene.LightManager.Count; i++)
                {
                    Light light = scene.LightManager.Lights[i];
                    if (light is DirectionalLight directional)
                    {
                        DebugDraw.DrawRay(light.Name + "0", light.Transform.GlobalPosition, light.Transform.Forward, false, Vector4.One);
                        DebugDraw.DrawQuadBillboard(light.Name, light.Transform.GlobalPosition, pos, Vector3.UnitY, forward, new(0.25f), Vector2.Zero, Vector2.One, Vector4.One, (nint)IconManager.GetIconByName("Light"));

                        if (drawLightBounds)
                        {
                            for (int j = 0; j < scene.Cameras.Count; j++)
                            {
                                for (int ji = 0; ji < 16; ji++)
                                {
                                    DebugDraw.DrawFrustum($"{light}{scene.Cameras[j]}{ji}", directional.ShadowFrustra[ji], Vector4.One);
                                }
                            }
                        }
                    }
                    if (light is Spotlight spotlight)
                    {
                        DebugDraw.DrawQuadBillboard(light.Name, light.Transform.GlobalPosition, pos, Vector3.UnitY, forward, new(0.25f), Vector2.Zero, Vector2.One, Vector4.One, (nint)IconManager.GetIconByName("Light"));
                        DebugDraw.DrawRay(light.Name, light.Transform.GlobalPosition, light.Transform.Forward * spotlight.Range, false, Vector4.One);

                        DebugDraw.DrawRing(light.Name + "0", light.Transform.GlobalPosition + light.Transform.Forward, spotlight.GetConeEllipse(1), Vector4.One);
                        DebugDraw.DrawRing(light.Name + "1", light.Transform.GlobalPosition + light.Transform.Forward * spotlight.Range, spotlight.GetConeEllipse(spotlight.Range), Vector4.One);
                        DebugDraw.DrawRing(light.Name + "2", light.Transform.GlobalPosition + light.Transform.Forward * spotlight.Range, spotlight.GetInnerConeEllipse(spotlight.Range), Vector4.One);

                        if (drawLightBounds)
                        {
                            DebugDraw.DrawFrustum(light.Name + "Bounds", spotlight.ShadowFrustum, Vector4.One);
                        }
                    }
                    if (light is PointLight pointLight)
                    {
                        DebugDraw.DrawQuadBillboard(light.Name, light.Transform.GlobalPosition, pos, Vector3.UnitY, forward, new(0.25f), Vector2.Zero, Vector2.One, Vector4.One, (nint)IconManager.GetIconByName("Light"));
                        DebugDraw.DrawRingBillboard(light.Name + "0", light.Transform.GlobalPosition, pos, Vector3.UnitY, forward, (new(0, 0.5f, 0), new(0.5f, 0, 0)), Vector4.One);

                        if (drawLightBounds)
                        {
                            DebugDraw.DrawBoundingBox(light.Name + "Bounds", pointLight.ShadowBox, Vector4.One);
                        }
                    }
                }
            }

            if (drawCameras)
            {
                for (int i = 0; i < scene.Cameras.Count; i++)
                {
                    var cam = scene.Cameras[i];
                    if (!Application.InDesignMode && CameraManager.Current == cam)
                    {
                        continue;
                    }
                    DebugDraw.DrawQuadBillboard(cam.Name, cam.Transform.GlobalPosition, pos, Vector3.UnitY, forward, new(0.25f), Vector2.Zero, Vector2.One, Vector4.One, (nint)IconManager.GetIconByName("Camera"));
                    DebugDraw.DrawFrustum(cam.Name, cam.Transform.NormalizedFrustum, Vector4.One);
                }
            }

            if (drawSkeletons)
            {
                // TODO: Implement skeleton debug view.
                /*
                for (int i = 0; i < scene.MeshManager.Count; i++)
                {
                    var mesh = scene.MeshManager.Meshes[i];
                    if (mesh.Bones == null || mesh.Bones.Length == 0)
                        continue;
                    for (int j = 0; j < mesh.Bones.Length; j++)
                    {
                        var skele = mesh.Animature;
                        var bone = mesh.Bones[j];
                        var noriginMtx = scene.FindByName(skele?.Relationships[bone.Name].ParentName)?.Transform.Local ?? Matrix4x4.Identity;
                        var ndestMtx = scene.FindByName(bone.Name)?.Transform.Local ?? Matrix4x4.Identity;
                        var originMtx = noriginMtx * skele?.GetTransform(skele?.Relationships[bone.Name].ParentName) ?? Matrix4x4.Identity;
                        var destMtx = ndestMtx * skele?.GetTransform(bone.Name) ?? Matrix4x4.Identity;
                        var origin = Vector3.Zero.ApplyMatrix(originMtx);
                        var dest = Vector3.Zero.ApplyMatrix(destMtx);
                        DebugDraw.DrawLine(bone.Name, origin, dest - origin, false, Vector4.One);
                    }
                }
                */
            }

            if (drawBoundingBoxes)
            {
                // TODO: Implement bounding box debug view.
            }

            if (drawBoundingSpheres)
            {
                // TODO: Implement bounding spheres debug view.
            }

            if (drawColliders)
            {
                for (int i = 0; i < scene.GameObjects.Count; i++)
                {
                    var node = scene.GameObjects[i];
                    Transform transform = scene.GameObjects[i].Transform;
                    for (int j = 0; j < scene.GameObjects[i].Components.Count; j++)
                    {
                        IComponent component = scene.GameObjects[i].Components[j];
                        if (component is BoxCollider box)
                        {
                            DebugDraw.DrawBox(node.Name + j, transform.GlobalPosition, transform.GlobalOrientation, box.Width, box.Height, box.Depth, Vector4.One);
                        }
                        if (component is SphereCollider sphere)
                        {
                            DebugDraw.DrawSphere(node.Name + j, transform.GlobalPosition, transform.GlobalOrientation, sphere.Radius, Vector4.One);
                        }
                        if (component is CapsuleCollider capsule)
                        {
                            DebugDraw.DrawCapsule(node.Name + j, transform.GlobalPosition, transform.GlobalOrientation, capsule.Radius, capsule.Length, Vector4.One);
                        }
                        if (component is CylinderCollider cylinder)
                        {
                            DebugDraw.DrawCylinder(node.Name + j, transform.GlobalPosition, transform.GlobalOrientation, cylinder.Radius, cylinder.Length, Vector4.One);
                        }
                        if (component is TriangleCollider triangle)
                        {
                            DebugDraw.DrawTriangle(node.Name + j, transform.GlobalPosition, transform.GlobalOrientation, triangle.Pos1, triangle.Pos2, triangle.Pos3, Vector4.One);
                        }
                        if (component is CompoundCollider compound)
                        {
                            DebugDraw.DrawSphere(node.Name + j, transform.GlobalPosition + compound.Center, Quaternion.Identity, 0.1f, new(1, 1, 0, 1));
                        }
                    }
                }
            }

            if (drawGimbal)
            {
                GameObject? element = GameObject.Selected.First();
                Camera? camera = CameraManager.Current;
                ImGuizmo.Enable(true);
                ImGuizmo.SetOrthographic(CameraManager.Dimension == CameraEditorDimension.Dim2D);
                if (camera == null)
                {
                    return;
                }

                if (element == null)
                {
                    return;
                }

                Matrix4x4 view = camera.Transform.View;
                Matrix4x4 proj = camera.Transform.Projection;
                Matrix4x4 transform = element.Transform.Global;

                if (ImGuizmo.Manipulate(ref view, ref proj, operation, mode, ref transform))
                {
                    gimbalGrabbed = true;
                    if (element.Transform.Parent == null)
                    {
                        element.Transform.Local = transform;
                    }
                    else
                    {
                        element.Transform.Local = transform * element.Transform.Parent.GlobalInverse;
                    }
                }
                else if (!ImGuizmo.IsUsing())
                {
                    if (gimbalGrabbed)
                    {
                        var oldValue = gimbalBefore;
                        History.Default.Push(element.Transform, oldValue, transform, SetMatrix, RestoreMatrix);
                    }
                    gimbalGrabbed = false;
                    gimbalBefore = element.Transform.Local;
                }
            }
        }

        private static void SetMatrix(object context)
        {
            var ctx = (HistoryContext<Transform, Matrix4x4>)context;
            ctx.Target.Local = ctx.NewValue;
        }

        private static void RestoreMatrix(object context)
        {
            var ctx = (HistoryContext<Transform, Matrix4x4>)context;
            ctx.Target.Local = ctx.OldValue;
        }
    }
}