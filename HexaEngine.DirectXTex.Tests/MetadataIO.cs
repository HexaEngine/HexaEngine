namespace HexaEngine.DirectXTex.Tests
{
    using HexaEngine.Core.Graphics.Textures;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public unsafe class MetadataIO
    {
        private const string DDSFilename = "assets\\textures\\test.dds";
        private const string HDRFilename = "assets\\textures\\test.hdr";
        private const string TGAFilename = "assets\\textures\\test.tga";
        private const string WICFilename = "assets\\textures\\test.png";

        private static byte[] LoadTexture(string path) => File.ReadAllBytes(path);

        [Fact]
        public void GetMetadataFromDDSMemoryAndFile()
        {
            TexMetadata metadata1;
            TexMetadata metadata2;

            Span<byte> bytes = LoadTexture(DDSFilename);
            DirectXTex.GetMetadataFromDDSMemory(bytes, DDSFlags.None, &metadata1);
            DirectXTex.GetMetadataFromDDSFile(DDSFilename, DDSFlags.None, &metadata2);

            Assert.Equal(metadata1, metadata2);
        }

        [Fact]
        public void GetMetadataFromHDRMemoryAndFile()
        {
            TexMetadata metadata1;
            TexMetadata metadata2;

            Span<byte> bytes = LoadTexture(HDRFilename);
            DirectXTex.GetMetadataFromHDRMemory(bytes, &metadata1);
            DirectXTex.GetMetadataFromHDRFile(HDRFilename, &metadata2);

            Assert.Equal(metadata1, metadata2);
        }

        [Fact]
        public void GetMetadataFromTGAMemoryAndFile()
        {
            TexMetadata metadata1;
            TexMetadata metadata2;

            Span<byte> bytes = LoadTexture(TGAFilename);
            DirectXTex.GetMetadataFromTGAMemory(bytes, TGAFlags.None, &metadata1);
            DirectXTex.GetMetadataFromTGAFile(TGAFilename, TGAFlags.None, &metadata2);

            Assert.Equal(metadata1, metadata2);
        }

        [Fact]
        public void GetMetadataFromWICMemoryAndFile()
        {
            TexMetadata metadata1;
            TexMetadata metadata2;

            Span<byte> bytes = LoadTexture(WICFilename);
            DirectXTex.GetMetadataFromWICMemory(bytes, WICFlags.NONE, &metadata1);
            DirectXTex.GetMetadataFromWICFile(WICFilename, WICFlags.NONE, &metadata2);

            Assert.Equal(metadata1, metadata2);
        }
    }
}