namespace HexaEngine.Resources
{
    using System;
    using System.Runtime.CompilerServices;

    public class ResourceRef
    {
        private IDisposable? _value;

        public ResourceRef(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public IDisposable? Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
            set
            {
                _value = value;
                ValueChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<IDisposable?>? ValueChanged;

        public void Dispose()
        {
            // This ensures no invalid resources are used if the code is listing to ValueChanged.
            var old = _value;
            Value = null;
            old?.Dispose();
        }
    }

    public class ResourceRefNotNull
    {
        private readonly ResourceRef resource;

        public ResourceRefNotNull(ResourceRef resourceRef)
        {
            resource = resourceRef;
            resource.ValueChanged += OnValueChanged;
        }

        ~ResourceRefNotNull()
        {
            resource.ValueChanged -= OnValueChanged;
        }

        public string Name => resource.Name;

        public IDisposable Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => resource.Value ?? throw new NullReferenceException(nameof(resource.Value));
            set => resource.Value = value;
        }

        private void OnValueChanged(object? sender, IDisposable? disposable)
        {
            ValueChanged?.Invoke(this, disposable ?? throw new NullReferenceException(nameof(resource.Value)));
        }

        public event EventHandler<IDisposable>? ValueChanged;

        public ResourceRef Resource => resource;
    }

    public class ResourceRef<T> where T : class, IDisposable
    {
        private readonly ResourceRef resource;

        public ResourceRef(ResourceRef resource)
        {
            this.resource = resource;
            resource.ValueChanged += OnValueChanged;
        }

        ~ResourceRef()
        {
            resource.ValueChanged -= OnValueChanged;
        }

        public string Name => resource.Name;

        public T? Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T?)resource.Value;
            set => resource.Value = value;
        }

        public ResourceRef Resource => resource;

        private void OnValueChanged(object? sender, IDisposable? disposable)
        {
            ValueChanged?.Invoke(this, disposable as T);
        }

        public event EventHandler<T?>? ValueChanged;
    }

    public class ResourceRefNotNull<T> where T : class, IDisposable
    {
        private readonly ResourceRefNotNull resource;

        public ResourceRefNotNull(ResourceRefNotNull resource)
        {
            this.resource = resource;
            resource.ValueChanged += OnValueChanged;
        }

        public ResourceRefNotNull(ResourceRef resource)
        {
            this.resource = new(resource);
            resource.ValueChanged += OnValueChanged;
        }

        public ResourceRefNotNull(ResourceRef<T> resource)
        {
            this.resource = new(resource.Resource);
            resource.ValueChanged += OnValueChanged;
        }

        public string Name => resource.Name;

        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T)resource.Value;
            set => resource.Value = value;
        }

        public ResourceRefNotNull Resource => resource;

        private void OnValueChanged(object? sender, IDisposable? disposable)
        {
            ValueChanged?.Invoke(this, (disposable as T) ?? throw new NullReferenceException());
        }

        public event EventHandler<T?>? ValueChanged;
    }
}