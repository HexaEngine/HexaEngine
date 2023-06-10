﻿namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Mathematics;
    using ImGuiNET;
    using ImNodesNET;
    using System.Numerics;

    public class OutputNode : Node
    {
        private Vector3 sc = new(2, 0, 0);
        private const float speed = 10;
        private static bool first = true;

#pragma warning disable CS8618 // Non-nullable field 'Out' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        public OutputNode(int id, bool removable, bool isStatic) : base(id, "Output", removable, isStatic)
#pragma warning restore CS8618 // Non-nullable field 'Out' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        {
            TitleColor = 0xc80023ff;
            TitleHoveredColor = 0xe40028ff;
            TitleSelectedColor = 0xff002dff;
            Camera = new();
            Camera.Fov = 90;
            Camera.Transform.Width = 1;
            Camera.Transform.Height = 1;
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = CreateOrGetPin(editor, "in", PinKind.Input, PinType.Float4, ImNodesPinShape.QuadFilled, 1);
            base.Initialize(editor);
        }

        public void InitTexture(IGraphicsDevice device)
        {
            Texture = new(device, TextureDescription.CreateTexture2DWithRTV(256, 256, 1, Format.R16G16B16A16Float));
            DepthStencil = new(device, 256, 256, Format.D32FloatS8X24UInt);
        }

        [JsonIgnore]
        public Pin Out;

        [JsonIgnore]
        public Texture? Texture;

        [JsonIgnore]
        public DepthStencil DepthStencil;

        [JsonIgnore]
        public Camera Camera;

        protected override void DrawContent()
        {
            if (Texture == null)
            {
                return;
            }

            ImGui.Image(Texture.ShaderResourceView?.NativePointer ?? 0, new(256, 256));

            if (IsHovered && ImGui.IsMouseDown(ImGuiMouseButton.Middle) || first)
            {
                Vector2 delta = Vector2.Zero;
                if (Mouse.IsDown(MouseButton.Middle))
                {
                    delta = Mouse.Delta;
                }

                float wheel = Mouse.DeltaWheel.Y;

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
            Texture?.Dispose();
            DepthStencil?.Dispose();
            base.Destroy();
        }
    }
}