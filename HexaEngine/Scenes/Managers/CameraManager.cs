namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core;
    using HexaEngine.Scenes;

    public static class CameraManager
    {
        private static readonly Camera editorCamera = new() { Far = 1000 };

        private static Camera? culling;
        private static Camera? overwrite;

        public const float Speed = 10F;
        public const float AngularSpeed = 20F;

        public static Camera EditorCamera => editorCamera;

        public static Camera? Current
        {
            get
            {
                return overwrite ?? (Application.InEditMode ? editorCamera : SceneManager.Current?.CurrentCamera);
            }
            set
            {
                overwrite = value;
            }
        }

        public static Camera? Culling
        {
            get => Application.InEditMode ? culling ?? Current : SceneManager.Current?.CurrentCamera;
            set => culling = value;
        }
    }
}