namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Graphics.Textures;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Specifies flags for texture loading operations.
    /// </summary>
    [Flags]
    public enum TextureLoaderFlags
    {
        /// <summary>
        /// No special flags. Default behavior.
        /// </summary>
        None = 0,

        /// <summary>
        /// Generate mipmaps for the loaded texture.
        /// </summary>
        GenerateMipMaps = 1,

        /// <summary>
        /// Scale the loaded texture.
        /// </summary>
        Scale = 2,
    }

    /// <summary>
    /// Provides methods for loading and initializing textures and scratch images.
    /// </summary>
    public interface ITextureLoader
    {
        /// <summary>
        /// Gets the graphics device associated with the texture loader.
        /// </summary>
        IGraphicsDevice Device { get; }

        /// <summary>
        /// Gets or sets the flags for texture loading operations.
        /// </summary>
        TextureLoaderFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the scaling factor for texture loading operations.
        /// </summary>
        float ScalingFactor { get; set; }

        /// <summary>
        /// Loads a scratch image from a file.
        /// </summary>
        /// <param name="filename">The path to the image file to load.</param>
        /// <returns>An <see cref="IScratchImage"/> representing the loaded image.</returns>
        IScratchImage LoadFormFile(string filename);

        /// <summary>
        /// Loads a scratch image from assets.
        /// </summary>
        /// <param name="filename">The filename of the image asset.</param>
        /// <returns>An <see cref="IScratchImage"/> representing the loaded image.</returns>
        IScratchImage LoadFormAssets(string filename);

        /// <summary>
        /// Loads a scratch image from memory stream.
        /// </summary>
        /// <param name="filename">The filename of the image.</param>
        /// <param name="stream">A <see cref="Stream"/> containing the image data.</param>
        /// <returns>An <see cref="IScratchImage"/> representing the loaded image.</returns>
        IScratchImage LoadFromMemory(string filename, Stream stream);

        /// <summary>
        /// Loads a scratch image from memory stream.
        /// </summary>
        /// <param name="format">The format of the image.</param>
        /// <param name="stream">A <see cref="Stream"/> containing the image data.</param>
        /// <returns>An <see cref="IScratchImage"/> representing the loaded image.</returns>
        IScratchImage LoadFromMemory(TexFileFormat format, Stream stream);

        /// <summary>
        /// Loads a scratch image from memory stream.
        /// </summary>
        /// <param name="format">The format of the image.</param>
        /// <param name="data">A pointer containing the image data.</param>
        /// <param name="length">The pointer length.</param>
        /// <returns>An <see cref="IScratchImage"/> representing the loaded image.</returns>
        unsafe IScratchImage LoadFromMemory(TexFileFormat format, byte* data, nuint length);

        /// <summary>
        /// Loads a 1D texture from a file using specified description.
        /// </summary>
        ITexture1D LoadTexture1D(TextureFileDescription desc);

        /// <summary>
        /// Loads a 2D texture from a file using specified description.
        /// </summary>
        ITexture2D LoadTexture2D(TextureFileDescription desc);

        /// <summary>
        /// Loads a 3D texture from a file using specified description.
        /// </summary>
        ITexture3D LoadTexture3D(TextureFileDescription desc);

        /// <summary>
        /// Loads a 1D texture from a file using specified parameters.
        /// </summary>
        ITexture1D LoadTexture1D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc);

        /// <summary>
        /// Loads a 2D texture from a file with the specified parameters.
        /// </summary>
        /// <param name="path">The path to the texture file to load.</param>
        /// <param name="usage">The intended usage of the texture.</param>
        /// <param name="bind">The bind flags for the texture.</param>
        /// <param name="cpuAccess">The CPU access flags for the texture.</param>
        /// <param name="misc">The miscellaneous resource flags for the texture.</param>
        /// <returns>An <see cref="ITexture2D"/> representing the loaded 2D texture.</returns>
        ITexture2D LoadTexture2D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc);

        /// <summary>
        /// Loads a 3D texture from a file with the specified parameters.
        /// </summary>
        /// <param name="path">The path to the texture file to load.</param>
        /// <param name="usage">The intended usage of the texture.</param>
        /// <param name="bind">The bind flags for the texture.</param>
        /// <param name="cpuAccess">The CPU access flags for the texture.</param>
        /// <param name="misc">The miscellaneous resource flags for the texture.</param>
        /// <returns>An <see cref="ITexture3D"/> representing the loaded 3D texture.</returns>
        ITexture3D LoadTexture3D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc);

        /// <summary>
        /// Loads a 1D texture from a file.
        /// </summary>
        /// <param name="path">The path to the texture file to load.</param>
        /// <returns>An <see cref="ITexture1D"/> representing the loaded 1D texture.</returns>
        ITexture1D LoadTexture1D(string path);

        /// <summary>
        /// Loads a 2D texture from a file.
        /// </summary>
        /// <param name="path">The path to the texture file to load.</param>
        /// <returns>An <see cref="ITexture2D"/> representing the loaded 2D texture.</returns>
        ITexture2D LoadTexture2D(string path);

        /// <summary>
        /// Loads a 3D texture from a file.
        /// </summary>
        /// <param name="path">The path to the texture file to load.</param>
        /// <returns>An <see cref="ITexture3D"/> representing the loaded 3D texture.</returns>
        ITexture3D LoadTexture3D(string path);

        /// <summary>
        /// Captures a texture to a scratch image using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to capture the texture.</param>
        /// <param name="resource">The resource to capture.</param>
        /// <returns>An <see cref="IScratchImage"/> containing the captured texture data.</returns>
        IScratchImage CaptureTexture(IGraphicsContext context, IResource resource);

        /// <summary>
        /// Initializes a scratch image with the specified metadata and flags.
        /// </summary>
        /// <param name="metadata">The metadata describing the image.</param>
        /// <param name="flags">Flags specifying how to initialize the image.</param>
        /// <returns>An <see cref="IScratchImage"/> initialized with the provided metadata and flags.</returns>
        IScratchImage Initialize(TexMetadata metadata, CPFlags flags);

        /// <summary>
        /// Initializes a scratch image for a 1D texture with the specified format, dimensions, and flags.
        /// </summary>
        IScratchImage Initialize1D(Format fmt, int length, int arraySize, int mipLevels, CPFlags flags = CPFlags.None);

        /// <summary>
        /// Initializes a scratch image for a 2D texture with the specified format, dimensions, and flags.
        /// </summary>
        IScratchImage Initialize2D(Format fmt, int width, int height, int arraySize, int mipLevels, CPFlags flags = CPFlags.None);

        /// <summary>
        /// Initializes a scratch image for a 3D texture with the specified format, dimensions, and flags.
        /// </summary>
        IScratchImage Initialize3D(Format fmt, int width, int height, int depth, int mipLevels, CPFlags flags = CPFlags.None);

        /// <summary>
        /// Initializes a scratch image for a cube map texture with the specified format, dimensions, and flags.
        /// </summary>
        IScratchImage InitializeCube(Format fmt, int width, int height, int nCubes, int mipLevels, CPFlags flags = CPFlags.None);

        /// <summary>
        /// Initializes a scratch image from an existing image.
        /// </summary>
        IScratchImage InitializeFromImage(IImage image, bool allow1D = false, CPFlags flags = CPFlags.None);

        /// <summary>
        /// Initializes a scratch image from an array of images.
        /// </summary>
        IScratchImage InitializeArrayFromImages(IImage[] images, bool allow1D = false, CPFlags flags = CPFlags.None);

        /// <summary>
        /// Initializes a scratch image for a cube map from an array of images.
        /// </summary>
        IScratchImage InitializeCubeFromImages(IImage[] images, CPFlags flags = CPFlags.None);

        /// <summary>
        /// Initializes a scratch image for a 3D texture from an array of images.
        /// </summary>
        IScratchImage Initialize3DFromImages(IImage[] images, int depth, CPFlags flags = CPFlags.None);
    }

    /// <summary>
    /// Represents a scratch image used for image processing and manipulation.
    /// </summary>
    public interface IScratchImage : IDisposable
    {
        /// <summary>
        /// Swaps the content of two scratch images.
        /// </summary>
        /// <param name="oldImage">The old scratch image to be replaced.</param>
        /// <param name="newImage">The new scratch image.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SwapImage(ref IScratchImage oldImage, IScratchImage newImage)
        {
            oldImage.Dispose();
            oldImage = newImage;
        }

        /// <summary>
        /// Gets the metadata of the scratch image.
        /// </summary>
        public TexMetadata Metadata { get; }

        /// <summary>
        /// Gets the number of images in the scratch image.
        /// </summary>
        public int ImageCount { get; }

        /// <summary>
        /// Creates a 1D texture from the scratch image.
        /// </summary>
        /// <param name="device">The graphics device to create the texture on.</param>
        /// <param name="usage">The intended usage of the texture.</param>
        /// <param name="bindFlags">The bind flags for the texture.</param>
        /// <param name="accessFlags">The CPU access flags for the texture.</param>
        /// <param name="miscFlag">The miscellaneous resource flags for the texture.</param>
        /// <returns>An <see cref="ITexture1D"/> representing the created 1D texture.</returns>
        public ITexture1D CreateTexture1D(IGraphicsDevice device, Usage usage, BindFlags bindFlags, CpuAccessFlags accessFlags, ResourceMiscFlag miscFlag);

        /// <summary>
        /// Creates a 2D texture from the scratch image.
        /// </summary>
        /// <param name="device">The graphics device to create the texture on.</param>
        /// <param name="usage">The intended usage of the texture.</param>
        /// <param name="bindFlags">The bind flags for the texture.</param>
        /// <param name="accessFlags">The CPU access flags for the texture.</param>
        /// <param name="miscFlag">The miscellaneous resource flags for the texture.</param>
        /// <returns>An <see cref="ITexture2D"/> representing the created 2D texture.</returns>
        public ITexture2D CreateTexture2D(IGraphicsDevice device, Usage usage, BindFlags bindFlags, CpuAccessFlags accessFlags, ResourceMiscFlag miscFlag);

        /// <summary>
        /// Creates a 2D texture from a specific image in the scratch image.
        /// </summary>
        /// <param name="device">The graphics device to create the texture on.</param>
        /// <param name="imageIndex">The index of the image to use for creating the texture.</param>
        /// <param name="usage">The intended usage of the texture.</param>
        /// <param name="bindFlags">The bind flags for the texture.</param>
        /// <param name="accessFlags">The CPU access flags for the texture.</param>
        /// <param name="miscFlag">The miscellaneous resource flags for the texture.</param>
        /// <returns>An <see cref="ITexture2D"/> representing the created 2D texture.</returns>
        public ITexture2D CreateTexture2D(IGraphicsDevice device, int imageIndex, Usage usage, BindFlags bindFlags, CpuAccessFlags accessFlags, ResourceMiscFlag miscFlag);

        /// <summary>
        /// Creates a 3D texture from the scratch image.
        /// </summary>
        /// <param name="device">The graphics device to create the texture on.</param>
        /// <param name="usage">The intended usage of the texture.</param>
        /// <param name="bindFlags">The bind flags for the texture.</param>
        /// <param name="accessFlags">The CPU access flags for the texture.</param>
        /// <param name="miscFlag">The miscellaneous resource flags for the texture.</param>
        /// <returns>An <see cref="ITexture3D"/> representing the created 3D texture.</returns>
        public ITexture3D CreateTexture3D(IGraphicsDevice device, Usage usage, BindFlags bindFlags, CpuAccessFlags accessFlags, ResourceMiscFlag miscFlag);

        /// <summary>
        /// Saves the scratch image to a file with the specified format and flags.
        /// </summary>
        /// <param name="path">The path to the file to save to.</param>
        /// <param name="format">The file format to use for saving.</param>
        /// <param name="flags">Additional saving flags.</param>
        public void SaveToFile(string path, TexFileFormat format, int flags);

        /// <summary>
        /// Saves the scratch image to a memory stream with the specified format and flags.
        /// </summary>
        /// <param name="stream">The memory stream to write to.</param>
        /// <param name="format">The file format to use for saving.</param>
        /// <param name="flags">Additional saving flags.</param>
        public void SaveToMemory(Stream stream, TexFileFormat format, int flags);

        /// <summary>
        /// Saves the scratch image to a blob with the specified format and flags.
        /// </summary>
        /// <param name="ppData">The data pointer of the texture.</param>
        /// <param name="pSize">The data pointer size.</param>
        /// <param name="format">The file format to use for saving.</param>
        /// <param name="flags">Additional saving flags.</param>
        unsafe void SaveToMemory(byte** ppData, nuint* pSize, TexFileFormat format, int flags);

        /// <summary>
        /// Resizes the scratch image with a specified scale factor and filtering flags.
        /// </summary>
        /// <param name="scale">The scale factor for resizing.</param>
        /// <param name="flags">The filtering flags for resizing.</param>
        /// <returns>A new <see cref="IScratchImage"/> containing the resized image.</returns>
        public IScratchImage Resize(float scale, TexFilterFlags flags);

        /// <summary>
        /// Resizes the scratch image to a specified width and height with filtering flags.
        /// </summary>
        /// <param name="width">The width of the resized image.</param>
        /// <param name="height">The height of the resized image.</param>
        /// <param name="flags">The filtering flags for resizing.</param>
        /// <returns>A new <see cref="IScratchImage"/> containing the resized image.</returns>
        public IScratchImage Resize(int width, int height, TexFilterFlags flags);

        /// <summary>
        /// Generates mipmaps for the scratch image with specified filtering flags.
        /// </summary>
        /// <param name="flags">The filtering flags for generating mipmaps.</param>
        /// <returns>A new <see cref="IScratchImage"/> containing the mipmapped image.</returns>
        public IScratchImage GenerateMipMaps(TexFilterFlags flags);

        /// <summary>
        /// Compresses the scratch image to a specified format with compression flags.
        /// </summary>
        /// <param name="device">The graphics device to perform the compression.</param>
        /// <param name="format">The target format for compression.</param>
        /// <param name="flags">The compression flags.</param>
        /// <returns>A new <see cref="IScratchImage"/> containing the compressed image.</returns>
        public IScratchImage Compress(IGraphicsDevice device, Format format, TexCompressFlags flags);

        /// <summary>
        /// Compresses the scratch image to a specified format with compression flags.
        /// </summary>
        /// <param name="format">The target format for compression.</param>
        /// <param name="flags">The compression flags.</param>
        /// <returns>A new <see cref="IScratchImage"/> containing the compressed image.</returns>
        public IScratchImage Compress(Format format, TexCompressFlags flags);

        /// <summary>
        /// Converts the scratch image to a specified format with filtering flags.
        /// </summary>
        /// <param name="format">The target format for conversion.</param>
        /// <param name="flags">The filtering flags for conversion.</param>
        /// <returns>A new <see cref="IScratchImage"/> containing the converted image.</returns>
        public IScratchImage Convert(Format format, TexFilterFlags flags);

        /// <summary>
        /// Decompresses the scratch image to a specified format.
        /// </summary>
        /// <param name="format">The target format for decompression.</param>
        /// <returns>A new <see cref="IScratchImage"/> containing the decompressed image.</returns>
        public IScratchImage Decompress(Format format);

        /// <summary>
        /// Overwrites the format of the scratch image with a new format.
        /// </summary>
        /// <param name="format">The new format to be used for the scratch image.</param>
        /// <returns><c>true</c> if the format was successfully overwritten; otherwise, <c>false</c>.</returns>
        public bool OverwriteFormat(Format format);

        /// <summary>
        /// Gets an array of images contained in the scratch image.
        /// </summary>
        /// <returns>An array of <see cref="IImage"/> objects representing the individual images.</returns>
        public IImage[] GetImages();

        /// <summary>
        /// Copies the contents of this scratch image to another scratch image.
        /// </summary>
        /// <param name="scratchImage">The target scratch image to copy to.</param>
        public void CopyTo(IScratchImage scratchImage);
    }
}