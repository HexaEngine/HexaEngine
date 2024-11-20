namespace HexaEngine.D3D12
{
    using Hexa.NET.DXGI;
    using System;
    using System.Collections.Generic;
    using Format = Hexa.NET.DXGI.Format;

    public struct PSOOutput : IEquatable<PSOOutput>
    {
        public uint NumRenderTargets;

        public Format RTVFormats_0;
        public Format RTVFormats_1;
        public Format RTVFormats_2;
        public Format RTVFormats_3;
        public Format RTVFormats_4;
        public Format RTVFormats_5;
        public Format RTVFormats_6;
        public Format RTVFormats_7;

        public Format DSVFormat;

        public SampleDesc SampleDesc;

        public override readonly bool Equals(object? obj)
        {
            return obj is PSOOutput output && Equals(output);
        }

        public readonly bool Equals(PSOOutput other)
        {
            return NumRenderTargets == other.NumRenderTargets &&
                   RTVFormats_0 == other.RTVFormats_0 &&
                   RTVFormats_1 == other.RTVFormats_1 &&
                   RTVFormats_2 == other.RTVFormats_2 &&
                   RTVFormats_3 == other.RTVFormats_3 &&
                   RTVFormats_4 == other.RTVFormats_4 &&
                   RTVFormats_5 == other.RTVFormats_5 &&
                   RTVFormats_6 == other.RTVFormats_6 &&
                   RTVFormats_7 == other.RTVFormats_7 &&
                   DSVFormat == other.DSVFormat &&
                   EqualityComparer<SampleDesc>.Default.Equals(SampleDesc, other.SampleDesc);
        }

        public override readonly int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(NumRenderTargets);
            hash.Add(RTVFormats_0);
            hash.Add(RTVFormats_1);
            hash.Add(RTVFormats_2);
            hash.Add(RTVFormats_3);
            hash.Add(RTVFormats_4);
            hash.Add(RTVFormats_5);
            hash.Add(RTVFormats_6);
            hash.Add(RTVFormats_7);
            hash.Add(DSVFormat);
            hash.Add(SampleDesc);
            return hash.ToHashCode();
        }

        public static bool operator ==(PSOOutput left, PSOOutput right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PSOOutput left, PSOOutput right)
        {
            return !(left == right);
        }
    }
}