namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Unsafes;
    using System;
    using System.Collections.Generic;

    public class CameraContainer : IDisposable
    {
        private readonly List<Camera> cameras = [];
        private UnsafeList<Pointer<byte>> cameraNames = [];
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
        public UnsafeList<Pointer<byte>> Names => cameraNames;

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

        public unsafe void Add(Camera camera)
        {
            cameras.Add(camera);
            cameraNames.Add(camera.Name.ToUTF8Ptr());
            camera.NameChanged += OnNameChanged;
        }

        public unsafe void Clear()
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].NameChanged -= OnNameChanged;
                Free(cameraNames[i]);
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

        public unsafe bool Remove(Camera camera)
        {
            camera.NameChanged -= OnNameChanged;
            int index = cameras.IndexOf(camera);
            if (index != -1)
            {
                cameras.RemoveAt(index);
                Free(cameraNames[index]);
                cameraNames.RemoveAt(index);
                return true;
            }
            return false;
        }

        private unsafe void OnNameChanged(GameObject gameObject, string name)
        {
            int index = cameras.IndexOf((Camera)gameObject);
            var oldName = cameraNames[index];
            cameraNames[index] = name.ToUTF8Ptr();
            Free(oldName);
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}