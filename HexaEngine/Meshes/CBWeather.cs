namespace HexaEngine.Meshes
{
    using System.Numerics;

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
}