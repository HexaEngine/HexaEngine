#define PROFILE

namespace HexaEngine.Scenes
{
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public class SceneCameraTracker
    {
        private bool moved;
        private Vector3 last;
        private float movementThreshold = 0.001f;

        public float MovementThreshold { get => movementThreshold; set => movementThreshold = value; }

        public bool HasMovedSignificantly => moved;

        internal bool Tick()
        {
            var camera = CameraManager.Current;
            if (camera == null) return false;
            var now = camera.Transform.GlobalPosition;
            var delta = now - last;
            last = now;
            return moved = delta.LengthSquared() > movementThreshold;
        }
    }
}