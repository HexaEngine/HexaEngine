namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using System;
    using System.IO;

    public class OpenGLTextureLoader : ITextureLoader
    {
        private readonly OpenGLGraphicsDevice device;

        public OpenGLTextureLoader(OpenGLGraphicsDevice device)
        {
            this.device = device;
        }

        public IGraphicsDevice Device => device;

        public TextureLoaderFlags Flags { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float ScalingFactor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IScratchImage CaptureTexture(IGraphicsContext context, IResource resource)
        {
            throw new NotImplementedException();
        }

        public IScratchImage Initialize(TexMetadata metadata, CPFlags flags)
        {
            throw new NotImplementedException();
        }

        public IScratchImage Initialize1D(Format fmt, int length, int arraySize, int mipLevels, CPFlags flags = CPFlags.None)
        {
            throw new NotImplementedException();
        }

        public IScratchImage Initialize2D(Format fmt, int width, int height, int arraySize, int mipLevels, CPFlags flags = CPFlags.None)
        {
            throw new NotImplementedException();
        }

        public IScratchImage Initialize3D(Format fmt, int width, int height, int depth, int mipLevels, CPFlags flags = CPFlags.None)
        {
            throw new NotImplementedException();
        }

        public IScratchImage Initialize3DFromImages(IImage[] images, int depth, CPFlags flags = CPFlags.None)
        {
            throw new NotImplementedException();
        }

        public IScratchImage InitializeArrayFromImages(IImage[] images, bool allow1D = false, CPFlags flags = CPFlags.None)
        {
            throw new NotImplementedException();
        }

        public IScratchImage InitializeCube(Format fmt, int width, int height, int nCubes, int mipLevels, CPFlags flags = CPFlags.None)
        {
            throw new NotImplementedException();
        }

        public IScratchImage InitializeCubeFromImages(IImage[] images, CPFlags flags = CPFlags.None)
        {
            throw new NotImplementedException();
        }

        public IScratchImage InitializeFromImage(IImage image, bool allow1D = false, CPFlags flags = CPFlags.None)
        {
            throw new NotImplementedException();
        }

        public IScratchImage LoadFormAssets(string filename)
        {
            throw new NotImplementedException();
        }

        public IScratchImage LoadFormFile(string filename)
        {
            throw new NotImplementedException();
        }

        public IScratchImage LoadFromMemory(string filename, Stream stream)
        {
            throw new NotImplementedException();
        }

        public ITexture1D LoadTexture1D(TextureFileDescription desc)
        {
            throw new NotImplementedException();
        }

        public ITexture1D LoadTexture1D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            throw new NotImplementedException();
        }

        public ITexture1D LoadTexture1D(string path)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(TextureFileDescription desc)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(string path)
        {
            throw new NotImplementedException();
        }

        public ITexture3D LoadTexture3D(TextureFileDescription desc)
        {
            throw new NotImplementedException();
        }

        public ITexture3D LoadTexture3D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            throw new NotImplementedException();
        }

        public ITexture3D LoadTexture3D(string path)
        {
            throw new NotImplementedException();
        }
    }
}