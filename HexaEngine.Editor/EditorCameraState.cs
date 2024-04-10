namespace HexaEngine.Editor
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class EditorCameraState
    {
        public EditorCameraState()
        {
            Mode = EditorCameraMode.Orbit;
            Dimension = EditorCameraDimension.Dim3D;
            FreePosition = Vector3.Zero;
            FreeRotation = Vector3.Zero;
            OrbitPosition = new(10, 0, 0);
            OrbitCenter = Vector3.Zero;
            Far = 100;
            Fov = MathUtil.PIDIV2; // 90° == rad pi/2
            Speed = 10;
        }

        public EditorCameraState(EditorCameraMode mode, EditorCameraDimension dimension, Vector3 freePosition, Vector3 freeRotation, Vector3 orbitPosition, Vector3 orbitCenter, float far, float fov, float speed)
        {
            Mode = mode;
            Dimension = dimension;
            FreePosition = freePosition;
            FreeRotation = freeRotation;
            OrbitPosition = orbitPosition;
            OrbitCenter = orbitCenter;
            Far = far;
            Fov = fov;
            Speed = speed;
        }

        public EditorCameraMode Mode { get; set; }

        public EditorCameraDimension Dimension { get; set; }

        public Vector3 FreePosition { get; set; }

        public Vector3 FreeRotation { get; set; }

        public Vector3 OrbitPosition { get; set; }

        public Vector3 OrbitCenter { get; set; }

        public float Far { get; set; }

        public float Fov { get; set; }

        public float Speed { get; set; }
    }
}