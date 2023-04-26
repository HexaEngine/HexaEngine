namespace HexaEngine.Core.Lights.Structs
{
    using HexaEngine.Core.Lights.Types;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public struct ShadowDirectionalLightData : IEquatable<ShadowDirectionalLightData>
    {
        public static readonly unsafe int CascadePointerOffset = sizeof(Matrix4x4) * 16;

        public Matrix4x4 View0;
        public Matrix4x4 View1;
        public Matrix4x4 View2;
        public Matrix4x4 View3;
        public Matrix4x4 View4;
        public Matrix4x4 View5;
        public Matrix4x4 View6;
        public Matrix4x4 View7;
        public Matrix4x4 View8;
        public Matrix4x4 View9;
        public Matrix4x4 View10;
        public Matrix4x4 View11;
        public Matrix4x4 View12;
        public Matrix4x4 View13;
        public Matrix4x4 View14;
        public Matrix4x4 View15;

        public float Cascade0;
        public float Cascade1;
        public float Cascade2;
        public float Cascade3;
        public float Cascade4;
        public float Cascade5;
        public float Cascade6;
        public float Cascade7;
        public float Cascade8;
        public float Cascade9;
        public float Cascade10;
        public float Cascade11;
        public float Cascade12;
        public float Cascade13;
        public float Cascade14;
        public float Cascade15;

        public DirectionalLightData Data;

        public ShadowDirectionalLightData(DirectionalLight light)
        {
            Data.Color = light.Color;
            Data.Direction = light.Transform.Forward;
        }

        public void Update(DirectionalLight light)
        {
            Data.Color = light.Color;
            Data.Direction = light.Transform.Forward;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Matrix4x4* GetViews()
        {
            fixed (ShadowDirectionalLightData* @this = &this)
            {
                return (Matrix4x4*)@this;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe float* GetCascades()
        {
            fixed (ShadowDirectionalLightData* @this = &this)
            {
                return (float*)((byte*)@this + CascadePointerOffset);
            }
        }

        public override string ToString()
        {
            return Data.Color.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj is ShadowDirectionalLightData data && Equals(data);
        }

        public bool Equals(ShadowDirectionalLightData other)
        {
            return View0.Equals(other.View0) &&
                   View1.Equals(other.View1) &&
                   View2.Equals(other.View2) &&
                   View3.Equals(other.View3) &&
                   View4.Equals(other.View4) &&
                   View5.Equals(other.View5) &&
                   View6.Equals(other.View6) &&
                   View7.Equals(other.View7) &&
                   View8.Equals(other.View8) &&
                   View9.Equals(other.View9) &&
                   View10.Equals(other.View10) &&
                   View11.Equals(other.View11) &&
                   View12.Equals(other.View12) &&
                   View13.Equals(other.View13) &&
                   View14.Equals(other.View14) &&
                   View15.Equals(other.View15) &&
                   Cascade0 == other.Cascade0 &&
                   Cascade1 == other.Cascade1 &&
                   Cascade2 == other.Cascade2 &&
                   Cascade3 == other.Cascade3 &&
                   Cascade4 == other.Cascade4 &&
                   Cascade5 == other.Cascade5 &&
                   Cascade6 == other.Cascade6 &&
                   Cascade7 == other.Cascade7 &&
                   Cascade8 == other.Cascade8 &&
                   Cascade9 == other.Cascade9 &&
                   Cascade10 == other.Cascade10 &&
                   Cascade11 == other.Cascade11 &&
                   Cascade12 == other.Cascade12 &&
                   Cascade13 == other.Cascade13 &&
                   Cascade14 == other.Cascade14 &&
                   Cascade15 == other.Cascade15 &&
                   Data.Equals(other.Data);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(View0);
            hash.Add(View1);
            hash.Add(View2);
            hash.Add(View3);
            hash.Add(View4);
            hash.Add(View5);
            hash.Add(View6);
            hash.Add(View7);
            hash.Add(View8);
            hash.Add(View9);
            hash.Add(View10);
            hash.Add(View11);
            hash.Add(View12);
            hash.Add(View13);
            hash.Add(View14);
            hash.Add(View15);
            hash.Add(Cascade0);
            hash.Add(Cascade1);
            hash.Add(Cascade2);
            hash.Add(Cascade3);
            hash.Add(Cascade4);
            hash.Add(Cascade5);
            hash.Add(Cascade6);
            hash.Add(Cascade7);
            hash.Add(Cascade8);
            hash.Add(Cascade9);
            hash.Add(Cascade10);
            hash.Add(Cascade11);
            hash.Add(Cascade12);
            hash.Add(Cascade13);
            hash.Add(Cascade14);
            hash.Add(Cascade15);
            hash.Add(Data);
            return hash.ToHashCode();
        }

        public static bool operator ==(ShadowDirectionalLightData left, ShadowDirectionalLightData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ShadowDirectionalLightData left, ShadowDirectionalLightData right)
        {
            return !(left == right);
        }
    }
}