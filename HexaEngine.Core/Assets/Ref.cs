namespace HexaEngine.Core.Assets
{
    public delegate void RefValueChangedHandler<T>(object sender, T? value);

    public class Ref<T> : IDisposable where T : class, IDisposable
    {
        private readonly object _lock = new();
        private T? value;

        public T? Value
        {
            get => value;
            set
            {
                lock (_lock)
                {
                    if (this.value == value)
                    {
                        return;
                    }

                    this.value = value;
                    ValueChanged?.Invoke(this, value);
                }
            }
        }

        public bool IsNull
        {
            get
            {
                lock (_lock)
                {
                    return value == null;
                }
            }
        }

        public event RefValueChangedHandler<T>? ValueChanged;

        public virtual void Dispose()
        {
            lock (_lock)
            {
                var tmp = value;
                Value = null; // Set property to raise value changed event.
                tmp?.Dispose();
            }
        }
    }
}