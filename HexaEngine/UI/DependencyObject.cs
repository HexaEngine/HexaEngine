namespace HexaEngine.UI
{
    public abstract class DependencyObject : DispatcherObject, IDependencyElement
    {
        public string? Name { get; set; }

        public DependencyObject? Parent { get; set; }

        public abstract Type DependencyObjectType { get; }

        /// <summary>
        /// Searches for the first <typeparamref name="T"/> in the tree including self testing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? FindFirstObject<T>() where T : DependencyObject
        {
            if (this is T tSelf)
            {
                return tSelf;
            }

            if (Parent is null)
            {
                return null;
            }

            if (Parent is T t)
            {
                return t;
            }

            return Parent.ResolveObject<T>();
        }

        public T? ResolveObject<T>() where T : DependencyObject
        {
            if (Parent is null)
            {
                return null;
            }

            if (Parent is T t)
            {
                return t;
            }

            return Parent.ResolveObject<T>();
        }

        public DependencyObject? ResolveObject(string name)
        {
            if (Parent is null)
            {
                return null;
            }

            if (Parent.Name == name)
            {
                return Parent;
            }

            return Parent.ResolveObject(name);
        }

        protected virtual void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        private readonly Dictionary<DependencyProperty, object?> properties = new();

        public object? GetValue(DependencyProperty dp)
        {
            PropertyMetadata metadata = dp.GetMetadata(this) ?? throw new ArgumentException($"'{dp.Name}' is not registered with type '{DependencyObjectType}'");

            if (properties.TryGetValue(dp, out var value))
            {
                return value;
            }

            return metadata.DefaultValue;
        }

        public void SetValue(DependencyProperty dp, object? value)
        {
            PropertyMetadata metadata = dp.GetMetadata(this) ?? throw new ArgumentException($"'{dp.Name}' is not registered with type '{DependencyObjectType}'");

            if (dp.ValidateValueCallback != null && !dp.ValidateValueCallback(value))
            {
                if (metadata.CoerceValueCallback == null)
                {
                    return;
                }

                value = metadata.CoerceValueCallback(this, value);
            }

            properties.TryGetValue(dp, out var oldValue);

            DependencyPropertyChangedEventArgs e = new(dp, oldValue, value);

            OnPropertyChanged(e);
            metadata.PropertyChangedCallback?.Invoke(this, e);

            if (metadata is FrameworkMetadata frameworkMetadata && this is UIElement element)
            {
                if (frameworkMetadata.AffectsMeasure)
                {
                    element.InvalidateMeasure();
                }

                if (frameworkMetadata.AffectsArrange)
                {
                    element.InvalidateArrange();
                }

                if (frameworkMetadata.AffectsRender)
                {
                    element.InvalidateVisual();
                }
            }

            if (metadata.DefaultValue != null && metadata.DefaultValue.Equals(value))
            {
                properties.Remove(dp);
                return;
            }

            properties[dp] = value;
        }

        public TType? GetValue<TType>(DependencyProperty<TType> dp)
        {
            PropertyMetadata metadata = dp.GetMetadata(this) ?? throw new ArgumentException($"'{dp.Name}' is not registered with type '{DependencyObjectType}'");

            if (properties.TryGetValue(dp, out var value))
            {
                return (TType?)value;
            }

            return (TType?)metadata.DefaultValue;
        }

        public void SetValue<TType>(DependencyProperty<TType> dp, TType? value)
        {
            PropertyMetadata metadata = dp.GetMetadata(this) ?? throw new ArgumentException($"'{dp.Name}' is not registered with type '{DependencyObjectType}'");

            if (dp.ValidateValueCallback != null && !dp.ValidateValueCallback(value))
            {
                if (metadata.CoerceValueCallback == null)
                {
                    return;
                }

                value = (TType?)metadata.CoerceValueCallback(this, value);
            }

            properties.TryGetValue(dp, out var oldValue);

            DependencyPropertyChangedEventArgs e = new(dp, oldValue, value);

            OnPropertyChanged(e);
            metadata.PropertyChangedCallback?.Invoke(this, e);

            if (metadata is FrameworkMetadata frameworkMetadata && this is UIElement element)
            {
                if (frameworkMetadata.AffectsMeasure)
                {
                    element.InvalidateMeasure();
                }

                if (frameworkMetadata.AffectsArrange)
                {
                    element.InvalidateArrange();
                }
            }

            if (metadata.DefaultValue != null && metadata.DefaultValue.Equals(value))
            {
                properties.Remove(dp);
                return;
            }

            properties[dp] = value;
        }

        public void ClearValue(DependencyProperty dp)
        {
            PropertyMetadata metadata = dp.GetMetadata(this) ?? throw new ArgumentException($"'{dp.Name}' is not registered with type '{DependencyObjectType}'");
            properties.TryGetValue(dp, out var oldValue);

            DependencyPropertyChangedEventArgs e = new(dp, oldValue, metadata.DefaultValue);

            OnPropertyChanged(e);
            metadata.PropertyChangedCallback?.Invoke(this, e);

            properties.Remove(dp);
        }

        public void CoerceValue(DependencyProperty dp)
        {
            PropertyMetadata metadata = dp.GetMetadata(this) ?? throw new ArgumentException($"'{dp.Name}' is not registered with type '{DependencyObjectType}'");

            if (metadata.CoerceValueCallback == null)
            {
                return;
            }

            if (!properties.TryGetValue(dp, out object? oldValue))
            {
                oldValue = metadata.DefaultValue;
            }

            var newValue = metadata.CoerceValueCallback.Invoke(this, oldValue);

            if (newValue != oldValue)
            {
                DependencyPropertyChangedEventArgs e = new(dp, oldValue, newValue);

                OnPropertyChanged(e);
                metadata.PropertyChangedCallback?.Invoke(this, e);
            }

            properties[dp] = newValue;
        }
    }
}