namespace HexaEngine.Volumes
{
    public enum VolumeMode
    {
        Local,
        Global,
    }

    public enum VolumeTransitionMode
    {
        Hard,
        Linear,
        Smoothstep,
        CustomCurve
    }

    public enum CurveMode
    {
        Curve1ControlPoint,
        Curve2ControlPoint,
        Curve3ControlPoint,
        Curve4ControlPoint,
        Curve5ControlPoint,
        Curve6ControlPoint,
        Curve7ControlPoint,
        Curve8ControlPoint,
    }

    public struct Curve
    {
        public CurveMode Mode;
        public float ControlPoint1;
        public float ControlPoint2;
        public float ControlPoint3;
        public float ControlPoint4;
        public float ControlPoint5;
        public float ControlPoint6;
        public float ControlPoint7;
        public float ControlPoint8;
    }
}