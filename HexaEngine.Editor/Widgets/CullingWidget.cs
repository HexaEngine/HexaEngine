namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.DebugDraw;
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Culling;
    using System.Numerics;

    public class CullingWidget : EditorWindow
    {
        private bool drawBoundingSpheres = false;
        protected override string Name { get; } = "Culling";

        private static Vector3 ExtractScale(Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.X = new Vector3(matrix.M11, matrix.M12, matrix.M13).Length();
            scale.Y = new Vector3(matrix.M21, matrix.M22, matrix.M23).Length();
            scale.Z = new Vector3(matrix.M31, matrix.M32, matrix.M33).Length();

            float det = matrix.GetDeterminant();
            if (det < 0)
            {
                scale.X = -scale.X;
            }

            return scale;
        }

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            var manager = CullingManager.Current;

            if (manager == null)
            {
                return;
            }

            var flags = (int)manager.CullingFlags;
            if (ImGui.CheckboxFlags("Enable", ref flags, (int)CullingFlags.Debug))
            {
                manager.CullingFlags |= CullingFlags.Debug;
                return; // we have to skip this frame.
            }

            var depthBias = manager.DepthBias;
            if (ImGui.InputFloat("Depth Bias", ref depthBias))
            {
                manager.DepthBias = depthBias;
            }

            CullingStats stats = manager.Stats;

            ImGui.Checkbox("Draw Bounding Spheres", ref drawBoundingSpheres);

            if (drawBoundingSpheres)
            {
                for (int i = 0; i < stats.InstanceCount; i++)
                {
                    var instance = stats.Instances[i];
                    var world = Matrix4x4.Transpose(instance.World);

                    var center = Vector3.Transform(instance.BoundingSphere.Center, world);
                    var radius = instance.BoundingSphere.Radius * ExtractScale(world).Length();
                    DebugDraw.DrawSphere(center, default, radius, Colors.White);
                }
            }

            var vertexCount = stats.VertexCount;

            uint fmt = 0;
            while (vertexCount > 1000)
            {
                vertexCount /= 1000;
                fmt++;
            }

            char suffix = fmt switch
            {
                0 => ' ',
                1 => 'K',
                _ => 'M',
            };

            ImGui.Text($"Draw Calls: {stats.DrawCalls}/{stats.ActualDrawCalls}, Instances: {stats.DrawInstanceCount}/{stats.ActualDrawInstanceCount}, Vertices: {vertexCount}{suffix}");

            bool changed = false;
            changed |= ImGui.CheckboxFlags("Frustum Culling", ref flags, (int)CullingFlags.Frustum);
            changed |= ImGui.CheckboxFlags("Occlusion Culling", ref flags, (int)CullingFlags.Occlusion);
            if (changed)
            {
                manager.CullingFlags = (CullingFlags)flags;
            }
        }
    }
}