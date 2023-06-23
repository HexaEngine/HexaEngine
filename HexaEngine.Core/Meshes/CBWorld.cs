namespace HexaEngine.Core.Meshes
{
    using HexaEngine.Core.Scenes;
    using System;
    using System.Numerics;

    public struct CBWorld
    {
        public Matrix4x4 World;
        public Matrix4x4 WorldInv;

        public CBWorld(GameObject mesh)
        {
            World = Matrix4x4.Transpose(mesh.Transform.Global);
            WorldInv = Matrix4x4.Transpose(mesh.Transform.GlobalInverse);
        }

        public CBWorld(Matrix4x4 transform)
        {
            Matrix4x4.Invert(transform, out var inverse);
            World = Matrix4x4.Transpose(transform);
            WorldInv = Matrix4x4.Transpose(inverse);
        }
    }

    public struct CBTessellation
    {
        public float MinFactor;
        public float MaxFactor;
        public float MinDistance;
        public float MaxDistance;

        public CBTessellation()
        {
            MinFactor = 1.0f;
            MaxFactor = 2.0f;
            MinDistance = 4.0f;
            MaxDistance = 50.0f;
        }

        public CBTessellation(float minFactor, float maxFactor, float minDistance, float maxDistance)
        {
            MinFactor = minFactor;
            MaxFactor = maxFactor;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }
    }

    public struct CBWeather
    {
        public Vector4 LightDir;
        public Vector4 LightColor;
        public Vector4 SkyColor;
        public Vector4 AmbientColor;
        public Vector4 WindDir;

        public float WindSpeed;
        public float Time;
        public float Crispiness;
        public float Curliness;

        public float Coverage;
        public float Absorption;
        public float CloudsBottomHeight;
        public float CloudsTopHeight;

        public float DensityFactor;
        public float CloudType;
        public Vector2 _padd;

        //sky parameters
        public Vector3 A;

        public float _paddA;
        public Vector3 B;
        public float _paddB;
        public Vector3 C;
        public float _paddC;
        public Vector3 D;
        public float _paddD;
        public Vector3 E;
        public float _paddE;
        public Vector3 F;
        public float _paddF;
        public Vector3 G;
        public float _paddG;
        public Vector3 H;
        public float _paddH;
        public Vector3 I;
        public float _paddI;
        public Vector3 Z;
        public float _paddZ;
    };

    public struct CBVoxel
    {
        public Vector3 GridCenter;
        public float DataSize;        // voxel half-extent in world space units
        public float DataSizeRCP;    // 1.0 / voxel-half extent
        public uint DataRes;         // voxel grid resolution
        public float DataResRCP;     // 1.0 / voxel grid resolution
        public uint NumCones;
        public float NumConesRCP;
        public float MaxDistance;
        public float RayStepSize;
        public uint Mips;
    };
}