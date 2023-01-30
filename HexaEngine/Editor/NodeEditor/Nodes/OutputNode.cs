namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using ImGuiNET;
    using ImNodesNET;
    using System.Collections.Generic;
    using System.Numerics;

    public class OutputNode : Node
    {
        private Vector3 sc = new(2, 0, 0);
        private const float speed = 20;
        private static bool first = true;

        public OutputNode(IGraphicsDevice device, NodeEditor graph, bool removable, bool isStatic) : base(graph, "Output", removable, isStatic)
        {
            Texture = new(device, TextureDescription.CreateTexture2DWithRTV(128, 128, 1));
            Out = CreatePin("in", PinKind.Input, PinType.VectorAny, ImNodesNET.PinShape.QuadFilled);
            Camera = new();
            Camera.Fov = 90;
            Camera.Transform.Width = 1;
            Camera.Transform.Height = 1;
        }

        public Pin Out;
        public Texture Texture;
        public Camera Camera;

        protected override void DrawContent()
        {
            ImGui.Image(Texture.ShaderResourceView?.NativePointer ?? 0, new(128, 128));

            if (ImGui.IsMouseDown(ImGuiMouseButton.Middle) || first)
            {
                Vector2 delta = Vector2.Zero;
                if (Mouse.IsDown(MouseButton.Middle))
                    delta = Mouse.Delta;

                float wheel = 0;
                if (Keyboard.IsDown(KeyCode.LShift))
                    wheel = Mouse.DeltaWheel.Y;

                // Only update the camera's position if the mouse got moved in either direction
                if (delta.X != 0f || delta.Y != 0f || wheel != 0f || first)
                {
                    sc.X += sc.X / 2 * -wheel;

                    // Rotate the camera left and right
                    sc.Y += -delta.X * Time.Delta * speed;

                    // Rotate the camera up and down
                    // Prevent the camera from turning upside down (1.5f = approx. Pi / 2)
                    sc.Z = Math.Clamp(sc.Z + delta.Y * Time.Delta * speed, -MathF.PI / 2, MathF.PI / 2);

                    first = false;

                    // Calculate the cartesian coordinates
                    Vector3 pos = SphereHelper.GetCartesianCoordinates(sc);
                    var orientation = Quaternion.CreateFromYawPitchRoll(-sc.Y, sc.Z, 0);
                    Camera.Transform.PositionRotation = (pos, orientation);
                    Camera.Transform.Recalculate();
                }
            }
        }

        public override void Destroy()
        {
            Texture.Dispose();
            base.Destroy();
        }
    }
}