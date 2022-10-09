namespace HexaEngine.Editor
{
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using HexaEngine.Objects.Components;
    using HexaEngine.Scenes;
    using System.Numerics;

    public static class Inspector
    {
        private static bool drawGrid = true;
        private static bool drawLights = true;
        private static bool drawCameras = true;
        private static bool drawSkeletons = true;
        private static bool drawColliders = true;
        private static bool enabled = true;

        public static bool Enabled { get => enabled; set => enabled = value; }

        public static bool DrawGrid { get => drawGrid; set => drawGrid = value; }

        public static bool DrawLights { get => drawLights; set => drawLights = value; }

        public static bool DrawCameras { get => drawCameras; set => drawCameras = value; }

        public static bool DrawSkeletons { get => drawSkeletons; set => drawSkeletons = value; }

        public static bool DrawColliders { get => drawColliders; set => drawColliders = value; }

        public static void Draw()
        {
            if (!enabled)
                return;

            var scene = SceneManager.Current;
            if (scene == null) return;

            if (drawGrid)
            {
                //DebugDraw.DrawGrid(10, 10, Vector4.Zero);
            }

            if (drawLights)
            {
                for (int i = 0; i < scene.Lights.Count; i++)
                {
                    Light light = scene.Lights[i];
                    if (light.Type == LightType.Directional)
                    {
                        DebugDraw.DrawRay(light.Transform.GlobalPosition, light.Transform.Forward, false, Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitX * 0.1f, Vector3.UnitZ * 0.1f, Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitX * 0.1f, Vector3.UnitY * 0.1f, Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitY * 0.1f, Vector3.UnitZ * 0.1f, Vector4.Zero);
                    }
                    if (light is Spotlight spotlight)
                    {
                        DebugDraw.DrawRay(light.Transform.GlobalPosition, light.Transform.Forward * 10, false, Vector4.One);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition + light.Transform.Forward, spotlight.GetConeEllipse(1), Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition + light.Transform.Forward * 10, spotlight.GetConeEllipse(10), Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition + light.Transform.Forward * 10, spotlight.GetInnerConeEllipse(10), Vector4.Zero);
                    }
                    if (light.Type == LightType.Point)
                    {
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitX * 0.1f, Vector3.UnitZ * 0.1f, Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitX * 0.1f, Vector3.UnitY * 0.1f, Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitY * 0.1f, Vector3.UnitZ * 0.1f, Vector4.Zero);
                    }
                }
            }

            if (drawCameras)
            {
                for (int i = 0; i < scene.Cameras.Count; i++)
                {
                    var cam = scene.Cameras[i];
                    DebugDraw.DrawFrustum(new BoundingFrustum(cam.Transform.View * MathUtil.PerspectiveFovLH(cam.Transform.Fov.ToRad(), cam.Transform.AspectRatio, 0.1f, 10)), Vector4.Zero);
                }
            }

            if (drawSkeletons)
            {
                for (int i = 0; i < scene.Meshes.Count; i++)
                {
                    var mesh = scene.Meshes[i];
                    if (mesh.Bones == null || mesh.Bones.Length == 0)
                        continue;
                    for (int j = 0; j < mesh.Bones.Length; j++)
                    {
                        var skele = mesh.Skeleton;
                        var bone = mesh.Bones[j];
                        var noriginMtx = scene.Find(skele?.Relationships[bone.Name].ParentName)?.Transform.Local ?? Matrix4x4.Identity;
                        var ndestMtx = scene.Find(bone.Name)?.Transform.Local ?? Matrix4x4.Identity;
                        var originMtx = noriginMtx * skele?.GetGlobalTransform(skele?.Relationships[bone.Name].ParentName) ?? Matrix4x4.Identity;
                        var destMtx = ndestMtx * skele?.GetGlobalTransform(bone.Name) ?? Matrix4x4.Identity;
                        var origin = Vector3.Zero.ApplyMatrix(originMtx);
                        var dest = Vector3.Zero.ApplyMatrix(destMtx);
                        DebugDraw.DrawLine(origin, dest - origin, false, Vector4.One);
                    }
                }
            }

            if (drawColliders)
            {
                for (int i = 0; i < scene.Nodes.Count; i++)
                {
                    Transform transform = scene.Nodes[i].Transform;
                    for (int j = 0; j < scene.Nodes[i].Components.Count; j++)
                    {
                        IComponent component = scene.Nodes[i].Components[j];
                        if (component is BoxCollider box)
                        {
                            DebugDraw.DrawBox(transform.GlobalPosition, transform.GlobalOrientation, box.Width, box.Height, box.Depth, Vector4.One);
                        }
                        if (component is SphereCollider sphere)
                        {
                            DebugDraw.DrawSphere(transform.GlobalPosition, transform.GlobalOrientation, sphere.Radius, Vector4.One);
                        }
                    }
                }
            }
        }
    }
}