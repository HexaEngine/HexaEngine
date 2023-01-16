namespace HexaEngine.Core.Lights
{
    using HexaEngine.Lights;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public unsafe struct CBDirectionalLightSD
    {
        public static readonly int CascadePointerOffset = sizeof(Matrix4x4) * 16;

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

        public Vector4 Color;
        public Vector3 Direction;
        public int padd;

        public CBDirectionalLightSD(DirectionalLight light)
        {
            Color = light.Color;
            Direction = light.Transform.Forward;
            padd = default;
        }

        public void Update(DirectionalLight light)
        {
            Color = light.Color;
            Direction = light.Transform.Forward;
            padd = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4* GetViews()
        {
            fixed (CBDirectionalLightSD* @this = &this)
            {
                return (Matrix4x4*)@this;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float* GetCascades()
        {
            fixed (CBDirectionalLightSD* @this = &this)
            {
                return (float*)((byte*)@this + CascadePointerOffset);
            }
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }
}