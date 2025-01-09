namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.DebugDraw;
    using Hexa.NET.ImGui;
    using Hexa.NET.Utilities.Text;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Extensions;
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
            if (ImGuiP.CheckboxFlags("Enable"u8, ref flags, (int)CullingFlags.Debug))
            {
                manager.CullingFlags |= CullingFlags.Debug;
                return; // we have to skip this frame.
            }

            var depthBias = manager.DepthBias;
            if (ImGui.InputFloat("Depth Bias"u8, ref depthBias))
            {
                manager.DepthBias = depthBias;
            }

            CullingStats stats = manager.Stats;

            ImGui.Checkbox("Draw Bounding Spheres"u8, ref drawBoundingSpheres);

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

            byte* buffer = stackalloc byte[2048];
            StrBuilder builder = new(buffer, 2048);

            builder.Reset();
            builder.Append("Draw Calls: "u8);
            builder.Append(stats.DrawCalls);
            builder.Append('/');
            builder.Append(stats.ActualDrawCalls);

            builder.Append(", Instances: "u8);
            builder.Append(stats.DrawInstanceCount);
            builder.Append('/');
            builder.Append(stats.ActualDrawInstanceCount);

            builder.Append(", Vertices: "u8);
            builder.AppendByteSize(vertexCount, false);
            builder.End();

            ImGui.Text(builder);

            bool changed = false;
            changed |= ImGuiP.CheckboxFlags("Frustum Culling"u8, ref flags, (int)CullingFlags.Frustum);
            changed |= ImGuiP.CheckboxFlags("Occlusion Culling"u8, ref flags, (int)CullingFlags.Occlusion);
            if (changed)
            {
                manager.CullingFlags = (CullingFlags)flags;
            }
        }
    }
}