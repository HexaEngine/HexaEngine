namespace HexaEngine.Editor.Icons
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.IO.Textures;
    using System.Numerics;
    using System.Text.RegularExpressions;

    public class Icon
    {
        private Texture2D icon = null!;
        private Point2 position;
        private Point2 size;
        private Vector2 uv0;
        private Vector2 uv1;
        private Vector4 tint = Vector4.One;

        internal unsafe Icon(IGraphicsDevice device, AtlasBuilder builder)
        {
            CreateDefault(device, builder);

            Name = string.Empty;
            Target = ".*";
            Priority = -100;
            Regex = new(".*", RegexOptions.Compiled);
        }

        public Icon(IconDescription description, Point2 position, Point2 size)
        {
            Description = description;
            this.position = position;
            this.size = size;

            if (description.Tint != null)
            {
                tint = ParseColor(description.Tint);
            }

            Name = description.Name;
            Target = description.Target;
            Priority = description.Priority;
            Regex = new(Regex.Escape(description.Target).Replace("\\*", ".*").Replace("\\^", "^").Replace("\\$", "$"), RegexOptions.Compiled);
        }

        private static Vector4 ParseColor(string s)
        {
            if (!s.StartsWith('#'))
            {
                throw new FormatException("Color must begin with a '#'.");
            }

            Vector4 color = Vector4.UnitW;

            static byte Convert(ReadOnlySpan<char> s)
            {
                byte result = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c >= '0' && c <= '9')
                    {
                        result = (byte)(result * 16 + (c - '0'));
                    }
                    else if (c >= 'A' && c <= 'F')
                    {
                        result = (byte)(result * 16 + (c - 'A' + 10));
                    }
                    else if (c >= 'a' && c <= 'f')
                    {
                        result = (byte)(result * 16 + (c - 'a' + 10));
                    }
                    else
                    {
                        throw new FormatException("Invalid hex number");
                    }
                }
                return result;
            }

            // skip #.
            var span = s.AsSpan(1);
            switch (span.Length)
            {
                case 3:
                    color.X = Convert(span[..1]) / 15f;
                    color.Y = Convert(span.Slice(1, 1)) / 15f;
                    color.Z = Convert(span.Slice(2, 1)) / 15f;
                    break;

                case 4:
                    color.X = Convert(span[..1]) / 15f;
                    color.Y = Convert(span.Slice(1, 1)) / 15f;
                    color.Z = Convert(span.Slice(2, 1)) / 15f;
                    color.W = Convert(span.Slice(3, 1)) / 15f;
                    break;

                case 6:
                    color.X = Convert(span[..2]) / 255f;
                    color.Y = Convert(span.Slice(1, 2)) / 255f;
                    color.Z = Convert(span.Slice(2, 2)) / 255f;
                    break;

                case 8:
                    color.X = Convert(span[..2]) / 255f;
                    color.Y = Convert(span.Slice(1, 2)) / 255f;
                    color.Z = Convert(span.Slice(2, 2)) / 255f;
                    color.W = Convert(span.Slice(3, 2)) / 255f;
                    break;

                default:
                    throw new FormatException("Color must be in the format #RGB #RGBA #RRGGBB #RRGGBBAA");
            }

            return color;
        }

        public Icon(IconDescription description)
        {
            if (description.Tint != null)
            {
                tint = ParseColor(description.Tint);
            }
            Description = description;
            Name = description.Name;
            Target = description.Target;
            Priority = description.Priority;
            Regex = new(Regex.Escape(description.Target).Replace("\\*", ".*").Replace("\\^", "^").Replace("\\$", "$"), RegexOptions.Compiled);
        }

        public string Name { get; }

        public string Target { get; }

        public Regex Regex { get; }

        public int Priority { get; }

        public Texture2D Texture { get => icon; protected internal set => icon = value; }

        public Point2 AtlasPos { get => position; protected internal set => position = value; }

        public Point2 Size { get => size; protected internal set => size = value; }

        public Vector2 UVStart => uv0;

        public Vector2 UVEnd => uv1;

        public Vector4 Tint => tint;

        public IconDescription Description { get; } = null!;

        public void SetAtlas(Texture2D texture)
        {
            icon = texture;
            Vector2 texel = new(1f / texture.Width, 1f / texture.Height);
            uv0 = (Vector2)position * texel;
            uv1 = (Vector2)(position + size) * texel;
        }

        public unsafe void CreateDefault(IGraphicsDevice device, AtlasBuilder builder)
        {
            TexMetadata metadata = new()
            {
                Width = 2,
                Height = 2,
                Format = Format.R32G32B32A32Float,
                ArraySize = 1,
                MipLevels = 1,
                Dimension = TexDimension.Texture2D,
                AlphaMode = TexAlphaMode.Unknown,
                Depth = 1,
                MiscFlags = TexMiscFlags.None,
            };

            IScratchImage image = device.TextureLoader.Initialize(metadata, 0);

            var pixels = (Vector4*)image.GetImages()[0].Pixels;

            pixels[0] = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
            pixels[1] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            pixels[2] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            pixels[3] = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);

            position = builder.Append(image, 2, 2);
            size = new(2);

            image.Dispose();
        }

        /// <summary>
        /// Displays an image centered vertically in the window with the specified size.
        /// </summary>
        /// <param name="size">The size of the image.</param>
        public void Image(Vector2 size)
        {
            ImGui.Image(this, size, uv0, uv1, tint);
        }

        /// <summary>
        /// Displays an image centered vertically in the window with the specified size.
        /// </summary>
        /// <param name="image">The identifier of the image to display.</param>
        /// <param name="size">The size of the image.</param>
        public void ImageCenteredV(Vector2 size)
        {
            var windowHeight = ImGui.GetWindowSize().Y;
            var imageHeight = size.Y;

            ImGui.SetCursorPosY((windowHeight - imageHeight) * 0.5f);
            ImGui.Image(this, size, uv0, uv1, tint);
        }

        /// <summary>
        /// Displays an image centered horizontally in the window with the specified size.
        /// </summary>
        /// <param name="size">The size of the image.</param>
        public void ImageCenteredH(Vector2 size)
        {
            var windowWidth = ImGui.GetWindowSize().X;
            var imageWidth = size.X;

            ImGui.SetCursorPosX((windowWidth - imageWidth) * 0.5f);
            ImGui.Image(this, size, uv0, uv1, tint);
        }

        /// <summary>
        /// Displays an image centered both vertically and horizontally in the window with the specified size.
        /// </summary>
        /// <param name="size">The size of the image.</param>
        public void ImageCenteredVH(Vector2 size)
        {
            var windowSize = ImGui.GetWindowSize();

            ImGui.SetCursorPos((windowSize - size) * 0.5f);
            ImGui.Image(this, size, uv0, uv1, tint);
        }

        public void AddImage(ImDrawListPtr drawListPtr, Vector2 min, Vector2 max)
        {
            drawListPtr.AddImage(this, min, max);
        }

        public static implicit operator ImTextureID(Icon icon)
        {
            return (ulong)icon.icon.SRV!.NativePointer;
        }

        public static implicit operator nint(Icon icon)
        {
            return icon.icon.SRV!.NativePointer;
        }
    }
}