namespace HexaEngine.DirectXTex
{
    using Silk.NET.DXGI;

    public unsafe struct TexMetadata
    {
        public ulong Width;

        /// <summary>
        /// Should be 1 for 1D textures
        /// </summary>
        public ulong Height;

        /// <summary>
        /// Should be 1 for 1D or 2D textures
        /// </summary>
        public ulong Depth;

        /// <summary>
        /// For cubemap, this is a multiple of 6
        /// </summary>
        public ulong ArraySize;

        public ulong MipLevels;
        public TexMiscFlags MiscFlags;
        public TexMiscFlags2 MiscFlags2;
        public Format Format;
        public TexDimension Dimension;

        public ulong ComputeIndex(ulong mip, ulong item, ulong slice)
        {
            fixed (TexMetadata* @this = &this)
            {
                return Native.ComputeIndex(@this, mip, item, slice);
            }
        }

        public bool IsCubemap()
        {
            fixed (TexMetadata* @this = &this)
            {
                return Native.IsCubemap(@this);
            }
        }

        public bool IsPMAlpha()
        {
            fixed (TexMetadata* @this = &this)
            {
                return Native.IsPMAlpha(@this);
            }
        }

        public void SetAlphaMode(TexAlphaMode mode)
        {
            fixed (TexMetadata* @this = &this)
            {
                Native.SetAlphaMode(@this, mode);
            }
        }

        public TexAlphaMode GetAlphaMode()
        {
            fixed (TexMetadata* @this = &this)
            {
                return Native.GetAlphaMode(@this);
            }
        }

        public bool IsVolumemap()
        {
            fixed (TexMetadata* @this = &this)
            {
                return Native.IsVolumemap(@this);
            }
        }
    }
}