namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Describes a depth-stencil buffer, including its format, binding options, usage, CPU access options, view options, and sample description.
    /// </summary>
    public struct DepthStencilDesc
    {
        /// <summary>
        /// Gets or sets the data format of the depth-stencil buffer.
        /// </summary>
        public Format Format;

        /// <summary>
        /// Gets or sets the binding options for the depth-stencil buffer.
        /// </summary>
        public BindFlags BindFlags;

        /// <summary>
        /// Gets or sets the intended usage of the depth-stencil buffer.
        /// </summary>
        public Usage Usage;

        /// <summary>
        /// Gets or sets the CPU access options for the depth-stencil buffer.
        /// </summary>
        public CpuAccessFlags CPUAccessFlags;

        /// <summary>
        /// Gets or sets the depth-stencil view options for the buffer.
        /// </summary>
        public DepthStencilViewFlags ViewFlags;

        /// <summary>
        /// Gets or sets the description of multi-sampling for the depth-stencil buffer.
        /// </summary>
        public SampleDescription SampleDescription;

        /// <summary>
        /// Gets the default depth-stencil buffer description with commonly used settings.
        /// </summary>
        public static DepthStencilDesc Default => new(Format.D32FloatS8X24UInt, BindFlags.DepthStencil, Usage.Default, CpuAccessFlags.None, DepthStencilViewFlags.None, SampleDescription.Default);

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilDesc"/> struct with specified parameters.
        /// </summary>
        /// <param name="format">The data format of the depth-stencil buffer.</param>
        /// <param name="bindFlags">The binding options for the depth-stencil buffer.</param>
        /// <param name="usage">The intended usage of the depth-stencil buffer.</param>
        /// <param name="cPUAccessFlags">The CPU access options for the depth-stencil buffer.</param>
        /// <param name="viewFlags">The depth-stencil view options for the buffer.</param>
        /// <param name="sampleDescription">The description of multi-sampling for the depth-stencil buffer.</param>
        public DepthStencilDesc(Format format, BindFlags bindFlags, Usage usage, CpuAccessFlags cPUAccessFlags, DepthStencilViewFlags viewFlags, SampleDescription sampleDescription)
        {
            Format = format;
            BindFlags = bindFlags;
            Usage = usage;
            CPUAccessFlags = cPUAccessFlags;
            ViewFlags = viewFlags;
            SampleDescription = sampleDescription;
        }
    }
}