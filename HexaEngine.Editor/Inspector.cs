﻿namespace HexaEngine.Editor
{
    using Hexa.NET.DebugDraw;
    using Hexa.NET.ImGuizmo;
    using Hexa.NET.Mathematics;
    using HexaEngine.Components.Physics.Collider;
    using HexaEngine.Core;
    using HexaEngine.Editor.Icons;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Physics;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public static class Inspector
    {
        private static bool inspectorEnabled = true;
        private static bool drawGimbal = true;
        private static bool drawGrid = true;
        private static EditorDrawLightsFlags drawLights = EditorDrawLightsFlags.DrawLights;
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

        private static int gridSize = 100;

        public static ImGuizmoOperation Operation { get => operation; set => operation = value; }

        public static ImGuizmoMode Mode { get => mode; set => mode = value; }

        public static bool Enabled { get => inspectorEnabled; set => inspectorEnabled = value; }

        public static bool DrawGimbal { get => drawGimbal; set => drawGimbal = value; }

        public static bool DrawGrid { get => drawGrid; set => drawGrid = value; }

        public static EditorDrawLightsFlags DrawLights { get => drawLights; set => drawLights = value; }

        public static bool DrawLightBounds { get => drawLightBounds; set => drawLightBounds = value; }

        public static bool DrawCameras { get => drawCameras; set => drawCameras = value; }

        public static bool DrawSkeletons { get => drawSkeletons; set => drawSkeletons = value; }

        public static bool DrawColliders { get => drawColliders; set => drawColliders = value; }

        public static bool DrawBoundingBoxes { get => drawBoundingBoxes; set => drawBoundingBoxes = value; }

        public static bool DrawBoundingSpheres { get => drawBoundingSpheres; set => drawBoundingSpheres = value; }

        public static int GridSize { get => gridSize; set => gridSize = value; }

        public static unsafe void Draw()
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
                if (EditorCameraController.Dimension == EditorCameraDimension.Dim3D)
                {
                    DebugDraw.DrawGrid(Matrix4x4.Identity, GridFlags.DrawAxis);
                }
                else if (EditorCameraController.Dimension == EditorCameraDimension.Dim2D)
                {
                    DebugDraw.DrawGrid(MathUtil.RotationYawPitchRoll(0, float.Pi / 2, 0), GridFlags.DrawAxis);
                }
            }

            if ((drawLights & EditorDrawLightsFlags.DrawLights) != 0)
            {
                var lightIcon = IconManager.GetIconByName("Light");
                var lightUVStart = lightIcon?.UVStart ?? default;
                var lightUVEnd = lightIcon?.UVEnd ?? default;
                var lightTint = lightIcon?.Tint ?? default;
                var lightIconPtr = lightIcon?.Texture.SRV?.NativePointer ?? default;

                for (int i = 0; i < scene.LightManager.Count; i++)
                {
                    LightSource light = scene.LightManager.Lights[i];
                    if ((drawLights & EditorDrawLightsFlags.NoDirectionalLights) == 0 && light is DirectionalLight directional)
                    {
                        DebugDraw.DrawRay(light.Transform.GlobalPosition, light.Transform.Forward, false, Vector4.One);
                        DebugDraw.DrawQuadBillboard(light.Transform.GlobalPosition, pos, Vector3.UnitY, forward, new(0.25f), lightUVStart, lightUVEnd, lightTint, lightIconPtr);

                        if (drawLightBounds)
                        {
                            for (int j = 0; j < scene.Cameras.Count; j++)
                            {
                                for (int ji = 0; ji < 8; ji++)
                                {
                                    //DebugDraw.DrawFrustum(directional.ShadowFrustra[ji], Vector4.One);
                                }
                            }
                        }
                    }

                    if ((drawLights & EditorDrawLightsFlags.NoSpotLights) == 0 && light is Spotlight spotlight)
                    {
                        DebugDraw.DrawQuadBillboard(light.Transform.GlobalPosition, pos, Vector3.UnitY, forward, new(0.25f), lightUVStart, lightUVEnd, Vector4.One, lightIconPtr);
                        DebugDraw.DrawRay(light.Transform.GlobalPosition, light.Transform.Forward * spotlight.Range, false, Vector4.One);

                        DebugDraw.DrawRing(light.Transform.GlobalPosition + light.Transform.Forward, spotlight.GetConeEllipse(1), Vector4.One);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition + light.Transform.Forward * spotlight.Range, spotlight.GetConeEllipse(spotlight.Range), Vector4.One);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition + light.Transform.Forward * spotlight.Range, spotlight.GetInnerConeEllipse(spotlight.Range), Vector4.One);

                        if (drawLightBounds)
                        {
                            //DebugDraw.DrawFrustum(spotlight.ShadowFrustum.Corners, Vector4.One);
                        }
                    }

                    if ((drawLights & EditorDrawLightsFlags.NoPointLights) == 0 && light is PointLight pointLight)
                    {
                        DebugDraw.DrawQuadBillboard(light.Transform.GlobalPosition, pos, Vector3.UnitY, forward, new(0.25f), lightUVStart, lightUVEnd, Vector4.One, lightIconPtr);
                        DebugDraw.DrawRingBillboard(light.Transform.GlobalPosition, pos, Vector3.UnitY, forward, (new(0, 0.5f, 0), new(0.5f, 0, 0)), Vector4.One);

                        if (drawLightBounds)
                        {
                            //DebugDraw.DrawBoundingBox(pointLight.ShadowBox.Min, pointLight., Vector4.One);
                        }
                    }
                }
            }

            if (drawCameras)
            {
                var cameraIcon = IconManager.GetIconByName("Camera");
                var cameraUVStart = cameraIcon?.UVStart ?? default;
                var cameraUVEnd = cameraIcon?.UVEnd ?? default;
                var cameraTint = cameraIcon?.Tint ?? default;
                var cameraIconPtr = cameraIcon?.Texture.SRV?.NativePointer ?? default;

                for (int i = 0; i < scene.Cameras.Count; i++)
                {
                    var cam = scene.Cameras[i];
                    if (!Application.InEditMode && CameraManager.Current == cam)
                    {
                        continue;
                    }
                    DebugDraw.DrawQuadBillboard(cam.Transform.GlobalPosition, pos, Vector3.UnitY, forward, new(0.25f), cameraUVStart, cameraUVEnd, cameraTint, cameraIconPtr);
                    //DebugDraw.DrawFrustum(cam.Transform.NormalizedFrustum, Vector4.One);
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
                            DebugDraw.DrawBox(transform.GlobalPosition, transform.GlobalOrientation, box.Width, box.Height, box.Depth, Vector4.One);
                        }
                        if (component is SphereCollider sphere)
                        {
                            DebugDraw.DrawSphere(transform.GlobalPosition, transform.GlobalOrientation, sphere.Radius, Vector4.One);
                        }
                        if (component is CapsuleCollider capsule)
                        {
                            DebugDraw.DrawCapsule(transform.GlobalPosition, transform.GlobalOrientation, capsule.Radius, capsule.Length * 2, Vector4.One);
                        }
                        if (component is CharacterController controller)
                        {
                            switch (controller.Shape)
                            {
                                case CharacterControllerShape.Capsule:
                                    DebugDraw.DrawCapsule(transform.GlobalPosition, transform.GlobalOrientation, controller.CapsuleRadius, controller.CapsuleHeight * 2, Vector4.One);
                                    break;

                                case CharacterControllerShape.Box:
                                    DebugDraw.DrawBox(transform.GlobalPosition, transform.GlobalOrientation, controller.BoxWidth, controller.BoxHeight, controller.BoxDepth, Vector4.One);
                                    break;
                            }
                        }
                    }
                }
            }

            var gameObject = SelectionCollection.Global.First<GameObject>();

            if (drawGimbal && gameObject != null)
            {
                Camera? camera = CameraManager.Current;
                ImGuizmo.Enable(true);
                ImGuizmo.SetOrthographic(EditorCameraController.Dimension == EditorCameraDimension.Dim2D);
                if (camera == null)
                {
                    return;
                }

                Matrix4x4 view = camera.Transform.View;
                Matrix4x4 proj = camera.Transform.Projection;
                Matrix4x4 transform = gameObject.Transform.Global;

                if (ImGuizmo.Manipulate(ref view, ref proj, operation, mode, ref transform))
                {
                    gimbalGrabbed = true;
                    if (gameObject.Transform.Parent == null)
                    {
                        gameObject.Transform.SetMatrixOverwrite(transform);
                    }
                    else
                    {
                        gameObject.Transform.SetMatrixOverwrite(transform * gameObject.Transform.Parent.GlobalInverse);
                    }
                }
                else if (!ImGuizmo.IsUsing())
                {
                    if (gimbalGrabbed)
                    {
                        var oldValue = gimbalBefore;
                        History.Default.Push("Transform Object", gameObject.Transform, oldValue, transform, SetMatrix, RestoreMatrix);
                        scene.UnsavedChanged = true;
                    }
                    gimbalGrabbed = false;
                    gimbalBefore = gameObject.Transform.Local;
                }
            }
        }

        private static void SetMatrix(object context)
        {
            var ctx = (HistoryContext<Transform, Matrix4x4>)context;
            ctx.Target.SetMatrixOverwrite(ctx.NewValue);
        }

        private static void RestoreMatrix(object context)
        {
            var ctx = (HistoryContext<Transform, Matrix4x4>)context;
            ctx.Target.SetMatrixOverwrite(ctx.OldValue);
        }
    }
}