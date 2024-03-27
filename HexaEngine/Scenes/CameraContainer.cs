namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Unsafes;
    using System;
    using System.Collections.Generic;

    public class CameraContainer : IDisposable
    {
        private readonly List<Camera> cameras = [];
        private UnsafeList<StdString> cameraNames = [];
        private int activeCamera;
        private bool disposedValue;

        [JsonIgnore]
        public Camera this[int index]
        {
            get => cameras[index];
        }

        [JsonIgnore]
        public Camera? ActiveCamera
        {
            get => activeCamera >= 0 && activeCamera < cameras.Count ? cameras[activeCamera] : null;
            set
            {
                if (value == null)
                {
                    activeCamera = -1;
                    return;
                }
                activeCamera = cameras.IndexOf(value);
            }
        }

        [JsonIgnore]
        public UnsafeList<StdString> Names => cameraNames;

        public int ActiveCameraIndex { get => activeCamera; set => activeCamera = value; }

        [JsonIgnore]
        public int Count => cameras.Count;

        public void Add(GameObject gameObject)
        {
            if (gameObject is Camera camera)
            {
                Add(camera);
            }
        }

        public void Add(Camera camera)
        {
            cameras.Add(camera);
            cameraNames.Add(camera.Name);
            camera.OnNameChanged += OnNameChanged;
        }

        public void Clear()
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].OnNameChanged -= OnNameChanged;
                cameraNames[i].Release();
            }
            cameras.Clear();
            cameraNames.Clear();
        }

        public bool Remove(GameObject gameObject)
        {
            if (gameObject is Camera camera)
            {
                return Remove(camera);
            }
            return false;
        }

        public bool Remove(Camera camera)
        {
            camera.OnNameChanged -= OnNameChanged;
            int index = cameras.IndexOf(camera);
            if (index != -1)
            {
                cameras.RemoveAt(index);
                var name = cameraNames[index];
                cameraNames.RemoveAt(index);
                name.Release();
                return true;
            }
            return false;
        }

        private void OnNameChanged(GameObject gameObject, string name)
        {
            int index = cameras.IndexOf((Camera)gameObject);
            var oldName = cameraNames[index];
            cameraNames[index] = name;
            oldName.Release();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Clear();
                cameraNames.Release();
                disposedValue = true;
            }
        }

        ~CameraContainer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}