namespace HexaEngine.Editor.Icons
{
    using HexaEngine.Core.Graphics;
    using Hexa.NET.ImGui;
    using System.Numerics;
    using System.Text.RegularExpressions;

    public class Icon : IDisposable
    {
        private readonly Texture2D icon;
        private bool disposedValue;

        public unsafe Icon(IGraphicsDevice device)
        {
            Vector4[] pixels =
            [
                new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            ];

            fixed (Vector4* ppixels = pixels)
            {
                SubresourceData data = new(ppixels, sizeof(Vector4) * 2);
                Texture2DDescription desc = new(Format.R32G32B32A32Float, 2, 2, 1, 1, BindFlags.ShaderResource, Usage.Immutable);
                icon = new(device, desc, data);
            }

            Name = string.Empty;
            Target = ".*";
            Priority = -100;
            Regex = new(".*", RegexOptions.Compiled);
        }

        public Icon(IGraphicsDevice device, string name, string path, string target, int priority)
        {
            icon = new(device, new TextureFileDescription(path, TextureDimension.Texture2D, 1, Usage.Immutable));
            Name = name;
            Target = target;
            Priority = priority;
            Regex = new(Regex.Escape(target).Replace("\\*", ".*"), RegexOptions.Compiled);
        }

        public Icon(IGraphicsDevice device, IconDescription description)
        {
            icon = new(device, new TextureFileDescription(description.Path, TextureDimension.Texture2D, 1, Usage.Immutable));
            Name = description.Name;
            Target = description.Target;
            Priority = description.Priority;
            Regex = new(Regex.Escape(description.Target).Replace("\\*", ".*"), RegexOptions.Compiled);
        }

        public string Name { get; }

        public string Target { get; }

        public Regex Regex { get; }

        public int Priority { get; }

        public static implicit operator ImTextureID(Icon icon)
        {
            return icon.icon.SRV.NativePointer;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                icon.Dispose();
                disposedValue = true;
            }
        }

        ~Icon()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}