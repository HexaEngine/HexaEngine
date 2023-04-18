namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes.Components.Collider;
    using ImGuizmoNET;
    using System.Numerics;

    public static class Inspector
    {
        private static bool drawGrid = true;
        private static bool drawLights = true;
        private static bool drawCameras = true;
        private static bool drawSkeletons = true;
        private static bool drawBoundingBoxes = false;
        private static bool drawBoundingSpheres = false;
        private static bool drawColliders = true;
        private static bool enabled = true;

        private static ImGuizmoOperation operation = ImGuizmoOperation.TRANSLATE;
        private static ImGuizmoMode mode = ImGuizmoMode.LOCAL;
        private static bool gimbalGrabbed;
        private static Matrix4x4 gimbalBefore;
        private static bool drawLightBounds;

        public static ImGuizmoOperation Operation { get => operation; set => operation = value; }

        public static ImGuizmoMode Mode { get => mode; set => mode = value; }

        public static bool Enabled { get => enabled; set => enabled = value; }

        public static bool DrawGrid { get => drawGrid; set => drawGrid = value; }

        public static bool DrawLights { get => drawLights; set => drawLights = value; }

        public static bool DrawLightBounds { get => drawLightBounds; set => drawLightBounds = value; }

        public static bool DrawCameras { get => drawCameras; set => drawCameras = value; }

        public static bool DrawSkeletons { get => drawSkeletons; set => drawSkeletons = value; }

        public static bool DrawColliders { get => drawColliders; set => drawColliders = value; }

        public static bool DrawBoundingBoxes { get => drawBoundingBoxes; set => drawBoundingBoxes = value; }

        public static bool DrawBoundingSpheres { get => drawBoundingSpheres; set => drawBoundingSpheres = value; }

        public static unsafe void Draw()
        {
            if (!enabled)
                return;

            var scene = SceneManager.Current;
            if (scene == null) return;

            if (drawGrid)
            {
            }

            if (drawLights)
            {
                for (int i = 0; i < scene.Lights.Count; i++)
                {
                    Light light = scene.Lights.Lights[i];
                    if (light is DirectionalLight directional)
                    {
                        DebugDraw.DrawRay(light.Name + "0", light.Transform.GlobalPosition, light.Transform.Forward, false, Vector4.One);
                        DebugDraw.DrawSphere(light.Name + "1", light.Transform.GlobalPosition, Quaternion.Identity, 0.1f, Vector4.One);

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
                        DebugDraw.DrawRay(light.Name, light.Transform.GlobalPosition, light.Transform.Forward * spotlight.ShadowRange, false, Vector4.One);

                        DebugDraw.DrawRing(light.Name + "0", light.Transform.GlobalPosition + light.Transform.Forward, spotlight.GetConeEllipse(1), Vector4.One);
                        DebugDraw.DrawRing(light.Name + "1", light.Transform.GlobalPosition + light.Transform.Forward * spotlight.ShadowRange, spotlight.GetConeEllipse(spotlight.ShadowRange), Vector4.One);
                        DebugDraw.DrawRing(light.Name + "2", light.Transform.GlobalPosition + light.Transform.Forward * spotlight.ShadowRange, spotlight.GetInnerConeEllipse(spotlight.ShadowRange), Vector4.One);

                        if (drawLightBounds)
                        {
                            DebugDraw.DrawFrustum(light.Name + "Bounds", spotlight.ShadowFrustum, Vector4.One);
                        }
                    }
                    if (light is PointLight pointLight)
                    {
                        DebugDraw.DrawSphere(light.Name, light.Transform.GlobalPosition, Quaternion.Identity, 0.1f, Vector4.One);

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
                        continue;
                    DebugDraw.DrawFrustum(cam.Name, cam.Transform.Frustum, Vector4.One);
                }
            }

            if (drawSkeletons)
            {
                //TODO: Implement skeleton debug view.
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
                        var noriginMtx = scene.Find(skele?.Relationships[bone.Name].ParentName)?.Transform.Local ?? Matrix4x4.Identity;
                        var ndestMtx = scene.Find(bone.Name)?.Transform.Local ?? Matrix4x4.Identity;
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
                InstanceManager manager = scene.InstanceManager;
                for (int i = 0; i < manager.Instances.Count; i++)
                {
                    var instance = manager.Instances[i];
                    instance.GetBoundingBox(out var boundingBox);
                    DebugDraw.DrawBoundingBox(instance.ToString(), boundingBox, new(1, 1, 1, 0.4f));
                }
            }

            if (drawBoundingSpheres)
            {
                InstanceManager manager = scene.InstanceManager;
                for (int i = 0; i < manager.Instances.Count; i++)
                {
                    var instance = manager.Instances[i];
                    instance.GetBoundingSphere(out var sphere);
                    DebugDraw.DrawBoundingSphere(instance.ToString(), sphere, Vector4.One);
                }
            }

            if (drawColliders)
            {
                for (int i = 0; i < scene.Nodes.Count; i++)
                {
                    var node = scene.Nodes[i];
                    Transform transform = scene.Nodes[i].Transform;
                    for (int j = 0; j < scene.Nodes[i].Components.Count; j++)
                    {
                        IComponent component = scene.Nodes[i].Components[j];
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

            if (true)
            {
            }

            {
                GameObject? element = GameObject.Selected.First();
                Camera? camera = CameraManager.Current;
                ImGuizmo.Enable(true);
                ImGuizmo.SetOrthographic(false);
                if (camera == null) return;
                if (element == null) return;
                Matrix4x4 view = camera.Transform.View;
                Matrix4x4 proj = camera.Transform.Projection;
                Matrix4x4 transform = element.Transform.Global;

                if (ImGuizmo.Manipulate(ref view, ref proj, operation, mode, ref transform))
                {
                    gimbalGrabbed = true;
                    if (element.Transform.Parent == null)
                        element.Transform.Local = transform;
                    else
                        element.Transform.Local = transform * element.Transform.Parent.GlobalInverse;
                }
                else if (!ImGuizmo.IsUsing())
                {
                    if (gimbalGrabbed)
                    {
                        var oldValue = gimbalBefore;
                        Designer.History.Push(element.Transform, oldValue, transform, SetMatrix, RestoreMatrix);
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